using CryptoJackpot.Wallet.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Wallet.Application.Validators;

public class CreateUserCryptoWalletCommandValidator : AbstractValidator<CreateUserCryptoWalletCommand>
{
    public CreateUserCryptoWalletCommandValidator()
    {
        RuleFor(c => c.UserGuid)
            .NotEmpty().WithMessage("UserGuid is required");

        RuleFor(c => c.Address)
            .NotEmpty().WithMessage("Wallet address is required")
            .MaximumLength(256).WithMessage("Wallet address must not exceed 256 characters");

        RuleFor(c => c.CurrencySymbol)
            .NotEmpty().WithMessage("Currency symbol is required")
            .MaximumLength(20).WithMessage("Currency symbol must not exceed 20 characters");

        RuleFor(c => c.CurrencyName)
            .NotEmpty().WithMessage("Currency name is required")
            .MaximumLength(100).WithMessage("Currency name must not exceed 100 characters");

        RuleFor(c => c.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL must not exceed 500 characters")
            .When(c => c.LogoUrl is not null);

        RuleFor(c => c.Label)
            .NotEmpty().WithMessage("Label is required")
            .MaximumLength(100).WithMessage("Label must not exceed 100 characters");
    }
}
