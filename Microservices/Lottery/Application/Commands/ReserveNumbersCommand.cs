using CryptoJackpot.Lottery.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Commands;

public class ReserveNumbersCommand : IRequest<Result<List<LotteryNumberDto>>>
{
    public Guid LotteryId { get; set; }
    public Guid TicketId { get; set; }
    public List<int> Numbers { get; set; } = [];
    public int Series { get; set; }
}
