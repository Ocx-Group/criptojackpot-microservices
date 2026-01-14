using MediatR;

namespace CryptoJackpot.Lottery.Application.Events;

/// <summary>
/// Domain event triggered when a lottery is created.
/// Used to generate lottery numbers asynchronously.
/// </summary>
public class LotteryCreatedEvent : INotification
{
    public Guid LotteryId { get; set; }
    public int MinNumber { get; set; }
    public int MaxNumber { get; set; }
    public int TotalSeries { get; set; }
    public DateTime CreatedAt { get; set; }
}

