using CryptoJackpot.Lottery.Application.Commands;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class ReserveNumbersCommandValidator : AbstractValidator<ReserveNumbersCommand>
{
    public ReserveNumbersCommandValidator()
    {
        RuleFor(c => c.LotteryId)
            .NotEmpty().WithMessage("LotteryId is required");

        RuleFor(c => c.TicketId)
            .NotEmpty().WithMessage("TicketId is required");

        RuleFor(c => c.Numbers)
            .NotEmpty().WithMessage("Numbers list is required")
            .Must(numbers => numbers.Count > 0).WithMessage("At least one number is required")
            .Must(numbers => numbers.Distinct().Count() == numbers.Count)
            .WithMessage("Numbers list cannot contain duplicates");

        RuleFor(c => c.Series)
            .GreaterThan(0).WithMessage("Series must be greater than 0");
    }
}
