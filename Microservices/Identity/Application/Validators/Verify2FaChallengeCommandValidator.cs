using CryptoJackpot.Identity.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Identity.Application.Validators;

public class Verify2FaChallengeCommandValidator : AbstractValidator<Verify2FaChallengeCommand>
{
    public Verify2FaChallengeCommandValidator()
    {
        RuleFor(x => x.ChallengeToken)
            .NotEmpty()
            .WithMessage("Challenge token is required.");

        // Either Code or RecoveryCode must be provided
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Code) || !string.IsNullOrWhiteSpace(x.RecoveryCode))
            .WithMessage("Either TOTP code or recovery code is required.");

        // If Code is provided, it must be 6 digits
        When(x => !string.IsNullOrWhiteSpace(x.Code), () =>
        {
            RuleFor(x => x.Code)
                .Length(6)
                .WithMessage("TOTP code must be 6 digits.")
                .Matches(@"^\d{6}$")
                .WithMessage("TOTP code must contain only digits.");
        });

        // If RecoveryCode is provided, validate format (XXXX-XXXX or XXXXXXXX)
        When(x => !string.IsNullOrWhiteSpace(x.RecoveryCode), () =>
        {
            RuleFor(x => x.RecoveryCode)
                .Matches(@"^[A-Z0-9]{4}-?[A-Z0-9]{4}$")
                .WithMessage("Recovery code format is invalid.");
        });
    }
}

