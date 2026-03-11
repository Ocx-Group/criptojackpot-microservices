using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
using CryptoJackpot.Notification.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Consumers;

public class ReferralBonusCreditedConsumer : IConsumer<ReferralBonusCreditedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReferralBonusCreditedConsumer> _logger;

    public ReferralBonusCreditedConsumer(IMediator mediator, ILogger<ReferralBonusCreditedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReferralBonusCreditedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Received ReferralBonusCreditedEvent for referrer {Email} — Amount: ${Amount}",
            message.ReferrerEmail, message.BonusAmount);

        await _mediator.Send(new SendReferralBonusNotificationCommand
        {
            ReferrerEmail    = message.ReferrerEmail,
            ReferrerName     = message.ReferrerName,
            ReferrerLastName = message.ReferrerLastName,
            ReferredName     = message.ReferredName,
            ReferredLastName = message.ReferredLastName,
            ReferralCode     = message.ReferralCode,
            BonusAmount      = message.BonusAmount,
            BalanceAfter     = message.BalanceAfter,
            TransactionGuid  = message.TransactionGuid,
            CreditedAt       = message.CreditedAt
        });
    }
}

