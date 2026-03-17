using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Winner.Domain.Enums;

namespace CryptoJackpot.Winner.Domain.Models;

public class LotteryWinner : BaseEntity
{
    public Guid WinnerGuid { get; set; } = Guid.NewGuid();

    // Lottery reference
    public Guid LotteryId { get; set; }
    public string LotteryTitle { get; set; } = null!;

    // Winning ticket details
    public int Number { get; set; }
    public int Series { get; set; }
    public Guid TicketGuid { get; set; }
    public decimal PurchaseAmount { get; set; }

    // Winner user info (snapshot)
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }

    // Prize info (snapshot)
    public string? PrizeName { get; set; }
    public decimal? PrizeEstimatedValue { get; set; }
    public string? PrizeImageUrl { get; set; }

    public WinnerStatus Status { get; set; } = WinnerStatus.Announced;
    public DateTime WonAt { get; set; } = DateTime.UtcNow;
}
