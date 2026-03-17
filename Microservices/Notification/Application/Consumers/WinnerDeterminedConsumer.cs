using CryptoJackpot.Domain.Core.IntegrationEvents.Winner;
using CryptoJackpot.Notification.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Consumers;

/// <summary>
/// Consumes WinnerDeterminedEvent to send a winner congratulations email.
/// </summary>
public class WinnerDeterminedConsumer : IConsumer<WinnerDeterminedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<WinnerDeterminedConsumer> _logger;

    public WinnerDeterminedConsumer(IMediator mediator, ILogger<WinnerDeterminedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WinnerDeterminedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Received WinnerDeterminedEvent for winner {WinnerGuid}, lottery {LotteryId}. Sending winner notification to {Email}",
            message.WinnerGuid, message.LotteryId, message.UserEmail);

        if (string.IsNullOrWhiteSpace(message.UserEmail))
        {
            _logger.LogWarning(
                "WinnerDeterminedEvent for winner {WinnerGuid} has no user email. Skipping winner notification.",
                message.WinnerGuid);
            return;
        }

        await _mediator.Send(new SendWinnerNotificationCommand
        {
            Email = message.UserEmail,
            UserName = message.UserName ?? "Winner",
            WinnerGuid = message.WinnerGuid,
            LotteryId = message.LotteryId,
            LotteryTitle = message.LotteryTitle,
            Number = message.Number,
            Series = message.Series,
            PrizeName = message.PrizeName,
            PrizeEstimatedValue = message.PrizeEstimatedValue,
            PrizeImageUrl = message.PrizeImageUrl,
            PurchaseAmount = message.PurchaseAmount,
            WonAt = message.WonAt
        });
    }
}

