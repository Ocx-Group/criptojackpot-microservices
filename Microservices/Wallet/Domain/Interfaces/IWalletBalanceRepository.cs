using CryptoJackpot.Wallet.Domain.Models;

namespace CryptoJackpot.Wallet.Domain.Interfaces;

public interface IWalletBalanceRepository
{
    /// <summary>Returns the USD balance row for a user, or null if none exists yet.</summary>
    Task<WalletBalance?> GetByUserAsync(Guid userGuid, CancellationToken cancellationToken = default);

    /// <summary>Creates the initial balance row for a new user (called on first credit).</summary>
    Task<WalletBalance> AddAsync(WalletBalance balance, CancellationToken cancellationToken = default);

    /// <summary>Marks an existing balance row as modified for the next SaveChanges.</summary>
    void Update(WalletBalance balance);
}
