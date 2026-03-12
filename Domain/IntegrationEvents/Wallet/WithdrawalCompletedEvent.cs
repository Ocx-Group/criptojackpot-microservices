using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;

/// <summary>
/// Integration event published by Order when a CoinPayments spend completes successfully.
/// Consumed by: Wallet microservice (to update withdrawal status to Completed).
/// </summary>
public class WithdrawalCompletedEvent : Event
{
    public Guid RequestGuid { get; set; }
    public Guid UserGuid { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
}
