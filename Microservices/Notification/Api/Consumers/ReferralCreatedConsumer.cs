using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Notification.Application.Commands;
using MassTransit;
using MediatR;

namespace CryptoJackpot.Notification.Api.Consumers;

/// <summary>
/// Consumes ReferralCreatedEvent from Kafka (published by Identity microservice)
/// </summary>
public class ReferralCreatedConsumer : IConsumer<ReferralCreatedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReferralCreatedConsumer> _logger;

    public ReferralCreatedConsumer(IMediator mediator, ILogger<ReferralCreatedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReferralCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received ReferralCreatedEvent for referrer {Email}", message.ReferrerEmail);

        await _mediator.Send(new SendReferralNotificationCommand
        {
            ReferrerEmail = message.ReferrerEmail,
            ReferrerName = message.ReferrerName,
            ReferrerLastName = message.ReferrerLastName,
            ReferredName = message.ReferredName,
            ReferredLastName = message.ReferredLastName,
            ReferralCode = message.ReferralCode
        });
    }
}
