using CryptoJackpot.Identity.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Identity.Application.Validators;

public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .MaximumLength(150).WithMessage("Email must not exceed 150 characters")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
