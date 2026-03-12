using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;

/// <summary>
/// Integration event published by Order when a CoinPayments spend fails.
/// Consumed by: Wallet microservice (to revert withdrawal status and refund funds).
/// </summary>
public class WithdrawalFailedEvent : Event
{
    public Guid RequestGuid { get; set; }
    public Guid UserGuid { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
