using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Queries;

public class IsNumberAvailableQuery : IRequest<Result<bool>>
{
    public Guid LotteryId { get; set; }
    public int Number { get; set; }
    public int Series { get; set; }
}

