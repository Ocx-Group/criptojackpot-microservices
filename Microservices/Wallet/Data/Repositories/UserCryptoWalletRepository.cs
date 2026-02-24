using CryptoJackpot.Wallet.Data.Context;
using CryptoJackpot.Wallet.Domain.Interfaces;
using CryptoJackpot.Wallet.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Wallet.Data.Repositories;

public class UserCryptoWalletRepository : IUserCryptoWalletRepository
{
    private readonly WalletDbContext _context;

    public UserCryptoWalletRepository(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserCryptoWallet>> GetByUserGuidAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        return await _context.UserCryptoWallets
            .Where(w => w.UserGuid == userGuid && w.DeletedAt == null)
            .OrderByDescending(w => w.IsDefault)
            .ThenByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserCryptoWallet?> GetByWalletGuidAsync(Guid walletGuid, CancellationToken cancellationToken = default)
    {
        return await _context.UserCryptoWallets
            .FirstOrDefaultAsync(w => w.WalletGuid == walletGuid && w.DeletedAt == null, cancellationToken);
    }

    public async Task<UserCryptoWallet> AddAsync(UserCryptoWallet wallet, CancellationToken cancellationToken = default)
    {
        var entry = await _context.UserCryptoWallets.AddAsync(wallet, cancellationToken);
        return entry.Entity;
    }

    public void Update(UserCryptoWallet wallet)
    {
        _context.UserCryptoWallets.Update(wallet);
    }

    public void Delete(UserCryptoWallet wallet)
    {
        _context.UserCryptoWallets.Remove(wallet);
    }
}
