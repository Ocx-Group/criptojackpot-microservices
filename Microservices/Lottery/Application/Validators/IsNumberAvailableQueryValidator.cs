using CryptoJackpot.Lottery.Application.Queries;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class IsNumberAvailableQueryValidator : AbstractValidator<IsNumberAvailableQuery>
{
    public IsNumberAvailableQueryValidator()
    {
        RuleFor(q => q.LotteryId)
            .NotEmpty().WithMessage("LotteryId is required");

        RuleFor(q => q.Number)
            .GreaterThan(0).WithMessage("Number must be greater than 0");

        RuleFor(q => q.Series)
            .GreaterThan(0).WithMessage("Series must be greater than 0");
    }
}

