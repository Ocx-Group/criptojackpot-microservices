using CryptoJackpot.Wallet.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Wallet.Application.Validators;

public class AdminCreditCommandValidator : AbstractValidator<AdminCreditCommand>
{
    public AdminCreditCommandValidator()
    {
        RuleFor(x => x.UserGuid)
            .NotEmpty().WithMessage("UserGuid is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
