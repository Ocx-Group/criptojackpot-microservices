using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Domain.Enums;

namespace CryptoJackpot.Wallet.Domain.Models;

/// <summary>
/// Represents a user's request to withdraw funds from their internal wallet to an external crypto wallet.
/// When created, the requested amount is blocked (debited) from the available balance as a Pending transaction.
/// </summary>
public class WithdrawalRequest : BaseEntity
{
    /// <summary>Public unique identifier for API exposure.</summary>
    public Guid RequestGuid { get; set; } = Guid.NewGuid();

    /// <summary>Owner of the withdrawal request.</summary>
    public Guid UserGuid { get; set; }

    /// <summary>Amount in USD to withdraw.</summary>
    public decimal Amount { get; set; }

    /// <summary>Current status of the withdrawal request.</summary>
    public WithdrawalRequestStatus Status { get; set; } = WithdrawalRequestStatus.Pending;

    /// <summary>The crypto wallet address to send funds to.</summary>
    public string WalletAddress { get; set; } = null!;

    /// <summary>Currency symbol of the target wallet (e.g., BTC, ETH, USDT).</summary>
    public string CurrencySymbol { get; set; } = null!;

    /// <summary>Human-readable currency name.</summary>
    public string CurrencyName { get; set; } = null!;

    /// <summary>Optional admin notes (e.g., rejection reason).</summary>
    public string? AdminNotes { get; set; }

    /// <summary>Timestamp when the request was processed (approved/rejected/completed).</summary>
    public DateTime? ProcessedAt { get; set; }

    // ── Navigation ──────────────────────────────────────────────────────────

    /// <summary>The wallet transaction that blocked the funds when this request was created.</summary>
    public WalletTransaction? Transaction { get; set; }
}
