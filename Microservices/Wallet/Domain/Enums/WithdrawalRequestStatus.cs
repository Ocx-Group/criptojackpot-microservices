namespace CryptoJackpot.Wallet.Domain.Enums;

/// <summary>
/// Lifecycle status of a withdrawal request.
/// </summary>
public enum WithdrawalRequestStatus
{
    /// <summary>Withdrawal requested, funds blocked from available balance. Awaiting admin review.</summary>
    Pending = 1,

    /// <summary>Withdrawal approved by admin and queued for processing.</summary>
    Approved = 2,

    /// <summary>Withdrawal rejected by admin. Blocked funds are returned to available balance.</summary>
    Rejected = 3,

    /// <summary>Withdrawal completed. Funds sent to the external wallet.</summary>
    Completed = 4,

    /// <summary>Withdrawal cancelled by the user. Blocked funds are returned to available balance.</summary>
    Cancelled = 5,
}
