using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Notification.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Consumers;

/// <summary>
/// Consumes OrderCompletedEvent to send a purchase confirmation email to the buyer.
/// </summary>
public class OrderCompletedConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderCompletedConsumer> _logger;

    public OrderCompletedConsumer(IMediator mediator, ILogger<OrderCompletedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Received OrderCompletedEvent for order {OrderId}. Sending purchase confirmation to {Email}",
            message.OrderId, message.UserEmail);

        if (string.IsNullOrWhiteSpace(message.UserEmail))
        {
            _logger.LogWarning(
                "OrderCompletedEvent for order {OrderId} has no user email. Skipping purchase confirmation.",
                message.OrderId);
            return;
        }

        await _mediator.Send(new SendPurchaseConfirmationCommand
        {
            Email = message.UserEmail,
            UserName = message.UserName,
            OrderId = message.OrderId,
            TransactionId = message.TransactionId,
            LotteryTitle = message.LotteryTitle,
            LotteryNo = message.LotteryNo,
            TotalAmount = message.TotalAmount,
            PurchaseDate = message.Timestamp,
            Tickets = message.Tickets.Select(t => new PurchasedTicketItemDto
            {
                Number = t.Number,
                Series = t.Series,
                Amount = t.Amount,
                IsGift = t.IsGift
            }).ToList()
        });
    }
}

