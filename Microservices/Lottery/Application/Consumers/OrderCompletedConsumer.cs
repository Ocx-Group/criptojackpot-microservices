using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Interfaces;
using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Lottery.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Consumers;

/// <summary>
/// Consumes OrderCompletedEvent to mark lottery numbers as sold permanently.
/// Broadcasts the sale via SignalR to all connected clients.
/// Includes idempotency checks to handle edge cases (e.g., payment arriving after timeout).
/// </summary>
public class OrderCompletedConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryNotificationService _notificationService;
    private readonly ILogger<OrderCompletedConsumer> _logger;

    public OrderCompletedConsumer(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryNotificationService notificationService,
        ILogger<OrderCompletedConsumer> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Received OrderCompletedEvent for Order {OrderId}. Confirming {Count} numbers as sold.",
            message.OrderId, message.LotteryNumberIds.Count);

        // IDEMPOTENCY CHECK: Verify numbers are still in Reserved status before confirming sale
        // Edge case: Payment arrives at second 301 (after 5 min timeout released the numbers)
        var numbers = await _lotteryNumberRepository.GetByIdsAsync(message.LotteryNumberIds);
        
        if (numbers.Count == 0)
        {
            _logger.LogWarning(
                "Order {OrderId}: No numbers found with the provided IDs. Order may have been processed already or numbers don't exist.",
                message.OrderId);
            return;
        }

        // Check if numbers are already sold (idempotency - duplicate message)
        var alreadySold = numbers.Where(n => n.Status == NumberStatus.Sold && n.TicketId == message.TicketId).ToList();
        if (alreadySold.Count == numbers.Count)
        {
            _logger.LogInformation(
                "Order {OrderId}: All numbers already sold to ticket {TicketId}. Duplicate message ignored (idempotent).",
                message.OrderId, message.TicketId);
            return;
        }

        // Check if numbers were released (timeout occurred before payment)
        var releasedNumbers = numbers.Where(n => n.Status == NumberStatus.Available).ToList();
        if (releasedNumbers.Any())
        {
            _logger.LogError(
                "Order {OrderId}: {Count} numbers were released before payment confirmation (timeout). " +
                "Numbers: [{Numbers}]. Payment arrived too late - refund may be required.",
                message.OrderId, 
                releasedNumbers.Count,
                string.Join(", ", releasedNumbers.Select(n => $"{n.Number}-S{n.Series}")));
            
            // TODO: Publish a PaymentRefundRequiredEvent for the Order Service to handle
            return;
        }

        // Check if numbers are sold to a different ticket (race condition - should not happen with proper locking)
        var soldToOther = numbers.Where(n => n.Status == NumberStatus.Sold && n.TicketId != message.TicketId).ToList();
        if (soldToOther.Any())
        {
            _logger.LogError(
                "Order {OrderId}: {Count} numbers were sold to a different ticket. Critical data integrity issue! " +
                "Numbers: [{Numbers}]",
                message.OrderId, 
                soldToOther.Count,
                string.Join(", ", soldToOther.Select(n => $"{n.Number}-S{n.Series} (Ticket: {n.TicketId})")));
            return;
        }

        // Only proceed with numbers that are still Reserved
        var reservedNumbers = numbers.Where(n => n.Status == NumberStatus.Reserved).ToList();
        if (reservedNumbers.Count == 0)
        {
            _logger.LogWarning(
                "Order {OrderId}: No numbers in Reserved status to confirm as sold.",
                message.OrderId);
            return;
        }

        var success = await _lotteryNumberRepository.ConfirmNumbersSoldAsync(
            reservedNumbers.Select(n => n.Id).ToList(), 
            message.TicketId);

        if (success)
        {
            // Broadcast via SignalR
            var soldNumbers = reservedNumbers.Select(n => new NumberStatusDto
            {
                NumberId = n.Id,
                Number = n.Number,
                Series = n.Series,
                Status = NumberStatus.Sold
            }).ToList();

            await _notificationService.NotifyNumbersSoldAsync(message.LotteryId, soldNumbers);

            _logger.LogInformation(
                "Successfully confirmed and broadcasted {Count} numbers as sold for Ticket {TicketId}. Transaction: {TransactionId}",
                soldNumbers.Count, message.TicketId, message.TransactionId);
        }
        else
        {
            _logger.LogError(
                "Failed to confirm numbers as sold for Order {OrderId}. Database update failed.",
                message.OrderId);
        }
    }
}

