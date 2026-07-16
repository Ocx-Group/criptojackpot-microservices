using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;

/// <summary>
/// Integration event published when an admin approves a withdrawal request.
/// Consumed by: Order microservice (to send funds via CoinPayments spend API).
/// </summary>
public class WithdrawalApprovedEvent : Event
{
    public Guid RequestGuid { get; set; }
    public Guid UserGuid { get; set; }
    public decimal Amount { get; set; }
    public string WalletAddress { get; set; } = null!;
    public string CurrencySymbol { get; set; } = null!;
    public string CurrencyName { get; set; } = null!;
    public string? AdminNotes { get; set; }
}
