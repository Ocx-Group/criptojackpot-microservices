using CryptoJackpot.Wallet.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Wallet.Application.Validators;

public class CreateWithdrawalRequestCommandValidator : AbstractValidator<CreateWithdrawalRequestCommand>
{
    public CreateWithdrawalRequestCommandValidator()
    {
        RuleFor(c => c.UserGuid)
            .NotEmpty().WithMessage("UserGuid is required.");

        RuleFor(c => c.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.")
            .PrecisionScale(18, 4, false).WithMessage("Amount cannot have more than 4 decimal places.");

        RuleFor(c => c.WalletGuid)
            .NotEmpty().WithMessage("WalletGuid is required.");

        RuleFor(c => c)
            .Must(c => !string.IsNullOrWhiteSpace(c.TwoFactorCode) || !string.IsNullOrWhiteSpace(c.EmailVerificationCode))
            .WithMessage("Either a two-factor code or an email verification code is required.");

        RuleFor(c => c.TwoFactorCode)
            .Length(6).WithMessage("Two-factor code must be 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Two-factor code must contain only digits.")
            .When(c => !string.IsNullOrWhiteSpace(c.TwoFactorCode));

        RuleFor(c => c.EmailVerificationCode)
            .Length(6).WithMessage("Email verification code must be 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Email verification code must contain only digits.")
            .When(c => !string.IsNullOrWhiteSpace(c.EmailVerificationCode));
    }
}
