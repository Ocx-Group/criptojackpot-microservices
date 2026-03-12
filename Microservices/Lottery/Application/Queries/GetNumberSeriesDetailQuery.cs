using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Queries;

public class GetNumberSeriesDetailQuery : IRequest<Result<NumberSeriesDetailDto>>
{
    public Guid LotteryId { get; set; }
    public int Number { get; set; }
}
