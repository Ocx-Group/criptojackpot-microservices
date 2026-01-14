using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;

/// <summary>
/// Integration event published when a lottery is created.
/// Consumed by: Lottery microservice (to generate lottery numbers asynchronously)
/// </summary>
public class LotteryCreatedEvent : Event
{
    public Guid LotteryId { get; set; }
    public int MinNumber { get; set; }
    public int MaxNumber { get; set; }
    public int TotalSeries { get; set; }
}

