using CryptoJackpot.Lottery.Application.Queries;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class GetAvailableNumbersQueryValidator : AbstractValidator<GetAvailableNumbersQuery>
{
    public GetAvailableNumbersQueryValidator()
    {
        RuleFor(q => q.LotteryId)
            .NotEmpty().WithMessage("LotteryId is required");

        RuleFor(q => q.Count)
            .GreaterThan(0).WithMessage("Count must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Count must not exceed 100");
    }
}

