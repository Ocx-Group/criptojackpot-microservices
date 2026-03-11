using CryptoJackpot.Wallet.Data.Context;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using CryptoJackpot.Wallet.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Wallet.Data.Repositories;

public class WithdrawalRequestRepository : IWithdrawalRequestRepository
{
    private readonly WalletDbContext _context;

    public WithdrawalRequestRepository(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<WithdrawalRequest?> GetByGuidAsync(Guid requestGuid, CancellationToken ct = default)
    {
        return await _context.WithdrawalRequests
            .FirstOrDefaultAsync(r => r.RequestGuid == requestGuid && r.DeletedAt == null, ct);
    }

    public async Task<WithdrawalRequest?> GetByGuidAndUserAsync(Guid requestGuid, Guid userGuid, CancellationToken ct = default)
    {
        return await _context.WithdrawalRequests
            .FirstOrDefaultAsync(r => r.RequestGuid == requestGuid && r.UserGuid == userGuid && r.DeletedAt == null, ct);
    }

    public async Task<List<WithdrawalRequest>> GetByUserAsync(Guid userGuid, WithdrawalRequestStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.WithdrawalRequests
            .Where(r => r.UserGuid == userGuid && r.DeletedAt == null);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
    }

    public async Task<bool> HasPendingRequestAsync(Guid userGuid, CancellationToken ct = default)
    {
        return await _context.WithdrawalRequests
            .AnyAsync(r => r.UserGuid == userGuid
                        && r.Status == WithdrawalRequestStatus.Pending
                        && r.DeletedAt == null, ct);
    }

    public async Task<WithdrawalRequest> AddAsync(WithdrawalRequest request, CancellationToken ct = default)
    {
        var entry = await _context.WithdrawalRequests.AddAsync(request, ct);
        return entry.Entity;
    }

    public void Update(WithdrawalRequest request)
    {
        _context.WithdrawalRequests.Update(request);
    }
}
