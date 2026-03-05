using CryptoJackpot.Order.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Order.Application.Validators;

public class PayOrderCommandValidator : AbstractValidator<PayOrderCommand>
{
    public PayOrderCommandValidator()
    {
        RuleFor(c => c.OrderId)
            .NotEmpty().WithMessage("OrderId is required");

        RuleFor(c => c.UserId)
            .GreaterThan(0).WithMessage("UserId is required");
    }
}
