using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Queries;

public class GetNumberBoardQuery : IRequest<Result<NumberBoardSummaryDto>>
{
    public Guid LotteryId { get; set; }
}
