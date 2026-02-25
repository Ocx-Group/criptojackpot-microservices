using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Models;
using FluentResults;
namespace CryptoJackpot.Wallet.Domain.Interfaces;
public interface IWalletService
{
    /// <summary>
    /// Inserts a transaction in the ledger and updates the user balance atomically.
    /// For debits, validates that the user has sufficient funds before proceeding.
    /// </summary>
    Task<Result<WalletTransaction>> ApplyTransactionAsync(
        Guid userGuid,
        decimal amount,
        WalletTransactionDirection direction,
        WalletTransactionType type,
        Guid? referenceId = null,
        string? description = null,
        CancellationToken cancellationToken = default);
}
