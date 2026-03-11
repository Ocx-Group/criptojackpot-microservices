using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
using CryptoJackpot.Notification.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Consumers;

public class ReferralCommissionCreditedConsumer : IConsumer<ReferralCommissionCreditedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReferralCommissionCreditedConsumer> _logger;

    public ReferralCommissionCreditedConsumer(IMediator mediator, ILogger<ReferralCommissionCreditedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReferralCommissionCreditedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Received ReferralCommissionCreditedEvent for referrer {Email} — Commission: ${Amount}",
            message.ReferrerEmail, message.CommissionAmount);

        await _mediator.Send(new SendReferralCommissionNotificationCommand
        {
            ReferrerEmail    = message.ReferrerEmail,
            ReferrerName     = message.ReferrerName,
            ReferrerLastName = message.ReferrerLastName,
            BuyerName        = message.BuyerName,
            LotteryTitle     = message.LotteryTitle,
            CommissionAmount = message.CommissionAmount,
            BalanceAfter     = message.BalanceAfter,
            TransactionGuid  = message.TransactionGuid,
            OrderId          = message.OrderId,
            CreditedAt       = message.CreditedAt
        });
    }
}

