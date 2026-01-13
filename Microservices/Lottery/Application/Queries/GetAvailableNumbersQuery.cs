using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Queries;

public class GetAvailableNumbersQuery : IRequest<Result<List<LotteryNumberDto>>>
{
    public Guid LotteryId { get; set; }
    public int Count { get; set; } = 10;
}
