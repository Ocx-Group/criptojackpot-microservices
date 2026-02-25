using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Wallet.Domain.Models;

/// <summary>
/// Internal USD balance for a user.
/// One row per user — updated atomically (optimistic concurrency) on every credit or debit.
/// </summary>
public class WalletBalance : BaseEntity
{
    /// <summary>Owner of this balance (references Identity service user).</summary>
    public Guid UserGuid { get; set; }

    /// <summary>
    /// Current available balance in USD.
    /// Balance = TotalEarned - TotalWithdrawn - TotalPurchased.
    /// This is what the user can spend or withdraw right now.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Cumulative USD earned. Never decremented.
    /// Includes: referral bonuses, purchase commissions from referred users, and lottery prizes.
    /// </summary>
    public decimal TotalEarned { get; set; }

    /// <summary>
    /// Cumulative USD sent to external wallets via withdrawal requests. Never decremented.
    /// </summary>
    public decimal TotalWithdrawn { get; set; }

    /// <summary>
    /// Cumulative USD spent on lottery ticket purchases using internal balance. Never decremented.
    /// </summary>
    public decimal TotalPurchased { get; set; }

    /// <summary>
    /// EF Core concurrency token.
    /// Prevents lost updates when two operations modify the same balance simultaneously.
    /// </summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    // ── Navigation ──────────────────────────────────────────────────────────

    /// <summary>All transactions that have affected this balance.</summary>
    public ICollection<WalletTransaction> Transactions { get; set; } = [];
}