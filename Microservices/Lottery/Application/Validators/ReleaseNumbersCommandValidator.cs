using CryptoJackpot.Lottery.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class ReleaseNumbersCommandValidator : AbstractValidator<ReleaseNumbersCommand>
{
    public ReleaseNumbersCommandValidator()
    {
        RuleFor(c => c.TicketId)
            .NotEmpty().WithMessage("TicketId is required");
    }
}

