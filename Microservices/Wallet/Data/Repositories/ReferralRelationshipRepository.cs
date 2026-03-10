using CryptoJackpot.Wallet.Data.Context;
using CryptoJackpot.Wallet.Domain.Interfaces;
using CryptoJackpot.Wallet.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Wallet.Data.Repositories;

public class ReferralRelationshipRepository : IReferralRelationshipRepository
{
    private readonly WalletDbContext _context;

    public ReferralRelationshipRepository(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<ReferralRelationship> AddAsync(ReferralRelationship relationship, CancellationToken cancellationToken = default)
    {
        var entry = await _context.ReferralRelationships.AddAsync(relationship, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task<ReferralRelationship?> GetByReferredUserGuidAsync(Guid referredUserGuid, CancellationToken cancellationToken = default)
    {
        return await _context.ReferralRelationships
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReferredUserGuid == referredUserGuid, cancellationToken);
    }

    public async Task<bool> ExistsByReferredUserGuidAsync(Guid referredUserGuid, CancellationToken cancellationToken = default)
    {
        return await _context.ReferralRelationships
            .AnyAsync(r => r.ReferredUserGuid == referredUserGuid, cancellationToken);
    }
}

