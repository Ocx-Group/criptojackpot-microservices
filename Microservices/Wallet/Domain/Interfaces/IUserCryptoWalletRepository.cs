using CryptoJackpot.Wallet.Domain.Models;

namespace CryptoJackpot.Wallet.Domain.Interfaces;

public interface IUserCryptoWalletRepository
{
    Task<List<UserCryptoWallet>> GetByUserGuidAsync(Guid userGuid, CancellationToken cancellationToken = default);
    Task<UserCryptoWallet?> GetByWalletGuidAsync(Guid walletGuid, CancellationToken cancellationToken = default);
    Task<UserCryptoWallet> AddAsync(UserCryptoWallet wallet, CancellationToken cancellationToken = default);
    void Update(UserCryptoWallet wallet);
    void Delete(UserCryptoWallet wallet);
}
