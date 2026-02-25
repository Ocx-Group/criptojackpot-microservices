namespace CryptoJackpot.Wallet.Domain.Enums;

/// <summary>
/// Lifecycle status of an internal wallet transaction.
/// </summary>
public enum WalletTransactionStatus
{
    /// <summary>Transaction created and awaiting processing (used for withdrawals pending blockchain confirmation).</summary>
    Pending = 1,

    /// <summary>Transaction successfully processed and reflected in the balance.</summary>
    Completed = 2,

    /// <summary>Transaction was rejected or failed (funds are not moved).</summary>
    Failed = 3,

    /// <summary>Transaction was reversed after being completed (e.g., refund).</summary>
    Reversed = 4,
}

