using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Notification.Application.Commands;
using MassTransit;
using MediatR;

namespace CryptoJackpot.Notification.Api.Consumers;

/// <summary>
/// Consumes PasswordResetRequestedEvent from Kafka (published by Identity microservice)
/// </summary>
public class PasswordResetRequestedConsumer : IConsumer<PasswordResetRequestedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<PasswordResetRequestedConsumer> _logger;

    public PasswordResetRequestedConsumer(IMediator mediator, ILogger<PasswordResetRequestedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PasswordResetRequestedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received PasswordResetRequestedEvent for {Email}", message.Email);

        await _mediator.Send(new SendPasswordResetCommand
        {
            Email = message.Email,
            Name = message.Name,
            LastName = message.LastName,
            SecurityCode = message.SecurityCode
        });
    }
}
