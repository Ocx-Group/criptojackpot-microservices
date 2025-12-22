using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Notification.Application.Commands;
using MassTransit;
using MediatR;

namespace CryptoJackpot.Notification.Api.Consumers;

/// <summary>
/// Consumes UserRegisteredEvent from Kafka (published by Identity microservice)
/// </summary>
public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(IMediator mediator, ILogger<UserRegisteredConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received UserRegisteredEvent for user {UserId}", message.UserId);

        await _mediator.Send(new SendEmailConfirmationCommand
        {
            UserId = message.UserId,
            Email = message.Email,
            Name = message.Name,
            LastName = message.LastName,
            Token = message.ConfirmationToken
        });
    }
}
