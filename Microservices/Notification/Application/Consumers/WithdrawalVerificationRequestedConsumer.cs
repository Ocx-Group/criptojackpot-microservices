using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
using CryptoJackpot.Notification.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Consumers;

public class WithdrawalVerificationRequestedConsumer : IConsumer<WithdrawalVerificationRequestedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<WithdrawalVerificationRequestedConsumer> _logger;

    public WithdrawalVerificationRequestedConsumer(IMediator mediator, ILogger<WithdrawalVerificationRequestedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WithdrawalVerificationRequestedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received WithdrawalVerificationRequestedEvent for {Email}", message.Email);

        await _mediator.Send(new SendWithdrawalVerificationCommand
        {
            Email = message.Email,
            Name = message.Name,
            LastName = message.LastName,
            SecurityCode = message.SecurityCode,
        });
    }
}
