using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Queries;

public class GetNumberStatsQuery : IRequest<Result<LotteryNumberStatsDto>>
{
    public Guid LotteryId { get; set; }
}

