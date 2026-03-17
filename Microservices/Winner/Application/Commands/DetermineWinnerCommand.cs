using CryptoJackpot.Winner.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Winner.Application.Commands;

public class DetermineWinnerCommand : IRequest<Result<WinnerDto>>
{
    public Guid LotteryId { get; set; }
    public string LotteryTitle { get; set; } = null!;
    public int Number { get; set; }
    public int Series { get; set; }

    // Optional prize snapshot data from frontend (lottery already has this info)
    public string? PrizeName { get; set; }
    public decimal? PrizeEstimatedValue { get; set; }
    public string? PrizeImageUrl { get; set; }
}
