using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Data.Context;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using CryptoJackpot.Wallet.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Wallet.Data.Repositories;

public class WalletTransactionRepository : IWalletRepository
{
    private readonly WalletDbContext _context;

    public WalletTransactionRepository(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<WalletTransaction> AddAsync(WalletTransaction transaction, CancellationToken cancellationToken = default)
    {
        var entry = await _context.WalletTransactions.AddAsync(transaction, cancellationToken);
        return entry.Entity;
    }

    public async Task<WalletTransaction?> GetByGuidAsync(Guid transactionGuid, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .FirstOrDefaultAsync(t => t.TransactionGuid == transactionGuid, cancellationToken);
    }

    public async Task<PagedList<WalletTransaction>> GetByUserAsync(
        Guid userGuid,
        WalletTransactionType? type,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WalletTransactions
            .Where(t => t.UserGuid == userGuid)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        query = query.OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<WalletTransaction>
        {
            Items = items,
            TotalItems = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ExistsByGuidAsync(Guid transactionGuid, CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .AnyAsync(t => t.TransactionGuid == transactionGuid, cancellationToken);
    }

    public async Task<(decimal TotalEarnings, decimal LastMonthEarnings)> GetReferralEarningsAsync(
        Guid userGuid, CancellationToken cancellationToken = default)
    {
        var referralTypes = new[]
        {
            WalletTransactionType.ReferralBonus,
            WalletTransactionType.ReferralPurchaseCommission
        };

        var cutoff = DateTime.UtcNow.AddMonths(-1);

        var query = _context.WalletTransactions
            .Where(t => t.UserGuid == userGuid
                        && referralTypes.Contains(t.Type)
                        && t.Direction == WalletTransactionDirection.Credit
                        && t.Status == WalletTransactionStatus.Completed);

        var totalEarnings = await query
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var lastMonthEarnings = await query
            .Where(t => t.CreatedAt >= cutoff)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        return (totalEarnings, lastMonthEarnings);
    }
}

