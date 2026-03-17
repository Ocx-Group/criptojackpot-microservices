using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Winner;

/// <summary>
/// Integration event published when a lottery winner is determined.
/// Consumed by: Notification service (to send winner congratulations email).
/// </summary>
public class WinnerDeterminedEvent : Event
{
    public Guid WinnerGuid { get; set; }
    public Guid LotteryId { get; set; }
    public string LotteryTitle { get; set; } = null!;
    public int Number { get; set; }
    public int Series { get; set; }
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? PrizeName { get; set; }
    public decimal? PrizeEstimatedValue { get; set; }
    public string? PrizeImageUrl { get; set; }
    public decimal PurchaseAmount { get; set; }
    public DateTime WonAt { get; set; }
}

