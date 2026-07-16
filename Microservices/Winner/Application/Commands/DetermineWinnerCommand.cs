using CryptoJackpot.Winner.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Winner.Application.Commands;

public class DetermineWinnerCommand : IRequest<Result<WinnerDto>>
{
    public Guid LotteryId { get; set; }
    public string LotteryTitle { get; set; } = null!;
    public int Number { get; set; }

    /// <summary>
    /// Zero-padded display representation of Number as shown to users
    /// (e.g. "007" for Pick3, "0007" for a 0-9999 raffle). Sent by the admin UI,
    /// which knows the lottery's type and number range.
    /// </summary>
    public string? NumberDisplay { get; set; }

    public int Series { get; set; }

    // Optional prize snapshot data from frontend (lottery already has this info)
    public string? PrizeName { get; set; }
    public decimal? PrizeEstimatedValue { get; set; }
    public string? PrizeImageUrl { get; set; }
    public int LotteryType { get; set; }
}
