using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Domain.Enums;

namespace CryptoJackpot.Wallet.Domain.Models;

/// <summary>
/// Internal USD wallet transaction ledger.
/// Each row is a single debit or credit in the user's internal balance.
/// Immutable by design — records are never updated, only inserted.
/// </summary>
public class WalletTransaction : BaseEntity
{
    /// <summary>Unique public identifier for idempotency checks.</summary>
    public Guid TransactionGuid { get; set; } = Guid.NewGuid();

    /// <summary>Owner of the internal wallet.</summary>
    public Guid UserGuid { get; set; }

    /// <summary>
    /// Positive amount in USD — direction is determined by <see cref="Direction"/>.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>Whether this movement adds or subtracts from the balance.</summary>
    public WalletTransactionDirection Direction { get; set; }

    /// <summary>Why this transaction exists.</summary>
    public WalletTransactionType Type { get; set; }

    /// <summary>Current lifecycle state of the transaction.</summary>
    public WalletTransactionStatus Status { get; set; } = WalletTransactionStatus.Completed;

    /// <summary>
    /// Balance snapshot in USD AFTER this transaction was applied.
    /// Allows reconstructing history without replaying the full ledger.
    /// </summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// Reference to the originating entity. Meaning depends on <see cref="Type"/>:
    /// - ReferralBonus / ReferralPurchaseCommission → ReferralId
    /// - LotteryPrize → WinnerId
    /// - TicketPurchase → OrderId
    /// - Withdrawal / WithdrawalRefund → WithdrawalRequestId
    /// </summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>Human-readable description shown to the user.</summary>
    public string? Description { get; set; }

    // ── Navigation ──────────────────────────────────────────────────────────

    /// <summary>Balance record updated by this transaction.</summary>
    public WalletBalance WalletBalance { get; set; } = null!;
}