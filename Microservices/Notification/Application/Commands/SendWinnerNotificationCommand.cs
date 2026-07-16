using FluentResults;
using MediatR;

namespace CryptoJackpot.Notification.Application.Commands;

/// <summary>
/// Command to send a winner congratulations email when a lottery winner is determined.
/// </summary>
public class SendWinnerNotificationCommand : IRequest<Result<bool>>
{
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public Guid WinnerGuid { get; set; }
    public Guid LotteryId { get; set; }
    public string LotteryTitle { get; set; } = null!;
    public int Number { get; set; }

    /// <summary>Zero-padded display representation of Number (e.g. "0007"). Null for legacy events.</summary>
    public string? DisplayNumber { get; set; }

    public int Series { get; set; }
    public string? PrizeName { get; set; }
    public decimal? PrizeEstimatedValue { get; set; }
    public string? PrizeImageUrl { get; set; }
    public decimal PurchaseAmount { get; set; }
    public DateTime WonAt { get; set; }
    public int LotteryType { get; set; }
}

