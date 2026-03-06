using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Domain.Constants;
using FluentValidation;

namespace CryptoJackpot.Order.Application.Validators;

public class ProcessWebhookCommandValidator : AbstractValidator<ProcessWebhookCommand>
{
    public ProcessWebhookCommandValidator()
    {
        RuleFor(c => c.InvoiceId)
            .NotEmpty().WithMessage("InvoiceId is required");

        RuleFor(c => c.EventType)
            .NotEmpty().WithMessage("EventType is required")
            .Must(eventType => CoinPaymentsWebhookEvents.All.Contains(eventType))
            .WithMessage("Invalid webhook event type");

        RuleFor(c => c.Payload)
            .NotNull().WithMessage("Webhook payload is required");
    }
}

