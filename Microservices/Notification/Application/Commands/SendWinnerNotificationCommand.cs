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
    public int Series { get; set; }
    public string? PrizeName { get; set; }
    public decimal? PrizeEstimatedValue { get; set; }
    public string? PrizeImageUrl { get; set; }
    public decimal PurchaseAmount { get; set; }
    public DateTime WonAt { get; set; }
}

