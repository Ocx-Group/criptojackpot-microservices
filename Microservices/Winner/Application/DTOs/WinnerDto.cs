using CryptoJackpot.Winner.Domain.Enums;

namespace CryptoJackpot.Winner.Application.DTOs;

public class WinnerDto
{
    public Guid WinnerGuid { get; set; }
    public Guid LotteryId { get; set; }
    public string LotteryTitle { get; set; } = null!;
    public int Number { get; set; }
    public int Series { get; set; }
    public Guid TicketGuid { get; set; }
    public decimal PurchaseAmount { get; set; }
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? PrizeName { get; set; }
    public decimal? PrizeEstimatedValue { get; set; }
    public string? PrizeImageUrl { get; set; }
    public WinnerStatus Status { get; set; }
    public DateTime WonAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LotteryType { get; set; }
}
