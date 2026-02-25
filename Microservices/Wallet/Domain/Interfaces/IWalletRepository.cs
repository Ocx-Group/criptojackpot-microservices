using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Models;

namespace CryptoJackpot.Wallet.Domain.Interfaces;

public interface IWalletRepository
{
    /// <summary>Append a new transaction entry (ledger is immutable — no updates).</summary>
    Task<WalletTransaction> AddAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>Retrieve a transaction by its public GUID.</summary>
    Task<WalletTransaction?> GetByGuidAsync(Guid transactionGuid, CancellationToken cancellationToken = default);

    /// <summary>Paginated transaction history for a user with optional type filter.</summary>
    Task<PagedList<WalletTransaction>> GetByUserAsync(
        Guid userGuid,
        WalletTransactionType? type,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Check whether a transaction with the given GUID already exists (idempotency guard).</summary>
    Task<bool> ExistsByGuidAsync(Guid transactionGuid, CancellationToken cancellationToken = default);

    /// <summary>Returns total and last-month referral earnings for a user.</summary>
    Task<(decimal TotalEarnings, decimal LastMonthEarnings)> GetReferralEarningsAsync(
        Guid userGuid, CancellationToken cancellationToken = default);
}
