using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Interfaces;
using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Lottery.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Consumers;

/// <summary>
/// Consumes OrderRevokedEvent to release lottery numbers that were marked as Sold
/// back to Available. This happens when an order was optimistically completed on
/// InvoicePending but the payment later failed (InvoiceTimedOut, InvoiceCancelled,
/// or InvoiceCompleted with insufficient amount).
/// Also decrements the SoldTickets counter on the LotteryDraw.
/// Broadcasts the release via SignalR to all connected clients.
/// </summary>
public class OrderRevokedConsumer : IConsumer<OrderRevokedEvent>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly ILotteryNotificationService _notificationService;
    private readonly ILogger<OrderRevokedConsumer> _logger;

    public OrderRevokedConsumer(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryDrawRepository lotteryDrawRepository,
        ILotteryNotificationService notificationService,
        ILogger<OrderRevokedConsumer> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _lotteryDrawRepository = lotteryDrawRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderRevokedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogWarning(
            "Received OrderRevokedEvent for Order {OrderId}. Reason: {Reason}. " +
            "Releasing {Count} sold numbers back to available.",
            message.OrderId, message.Reason, message.LotteryNumberIds.Count);

        var numbers = await _lotteryNumberRepository.GetByGuidsAsync(message.LotteryNumberIds);

        if (numbers.Count == 0)
        {
            _logger.LogWarning(
                "OrderRevokedConsumer: No numbers found for Order {OrderId}. Nothing to release.",
                message.OrderId);
            return;
        }

        // Release numbers regardless of current status (Sold or Reserved → Available)
        // This handles both cases: numbers already confirmed as Sold, or still in Reserved
        var numbersToRelease = numbers
            .Where(n => n.Status is NumberStatus.Sold or NumberStatus.Reserved)
            .ToList();

        if (numbersToRelease.Count == 0)
        {
            _logger.LogInformation(
                "OrderRevokedConsumer: All numbers for Order {OrderId} are already Available. " +
                "Revocation may have been processed already (idempotent).",
                message.OrderId);
            return;
        }

        // Release all matched numbers back to Available
        var releasedCount = await _lotteryNumberRepository.RevokeSoldNumbersByGuidsAsync(
            numbersToRelease.Select(n => n.LotteryNumberGuid).ToList());

        if (releasedCount > 0)
        {
            // Decrement SoldTickets counter (only for numbers that were Sold, not Reserved)
            var soldCount = numbersToRelease.Count(n => n.Status == NumberStatus.Sold);
            if (soldCount > 0)
            {
                await _lotteryDrawRepository.IncrementSoldTicketsAsync(message.LotteryId, -soldCount);
            }

            // Broadcast via SignalR
            var releasedNumbers = numbersToRelease.Select(n => new NumberStatusDto
            {
                NumberId = n.Id,
                Number = n.Number,
                Series = n.Series,
                Status = NumberStatus.Available
            }).ToList();

            await _notificationService.NotifyNumbersReleasedAsync(message.LotteryId, releasedNumbers);

            _logger.LogWarning(
                "OrderRevokedConsumer: Released {Count} numbers for revoked Order {OrderId}. " +
                "SoldTickets decremented by {SoldCount}. Reason: {Reason}",
                releasedCount, message.OrderId, soldCount, message.Reason);
        }
        else
        {
            _logger.LogError(
                "OrderRevokedConsumer: Failed to release numbers for revoked Order {OrderId}.",
                message.OrderId);
        }
    }
}

