using CryptoJackpot.Lottery.Application.Queries;
using FluentValidation;

namespace CryptoJackpot.Lottery.Application.Validators;

public class GetNumberStatsQueryValidator : AbstractValidator<GetNumberStatsQuery>
{
    public GetNumberStatsQueryValidator()
    {
        RuleFor(q => q.LotteryId)
            .NotEmpty().WithMessage("LotteryId is required");
    }
}

