using CryptoJackpot.Wallet.Domain.Models;

namespace CryptoJackpot.Wallet.Domain.Interfaces;

/// <summary>
/// Repository for referral relationship projections stored locally in Wallet.
/// </summary>
public interface IReferralRelationshipRepository
{
    /// <summary>Persist a new referral relationship.</summary>
    Task<ReferralRelationship> AddAsync(ReferralRelationship relationship, CancellationToken cancellationToken = default);

    /// <summary>Find the referrer for a given referred user.</summary>
    Task<ReferralRelationship?> GetByReferredUserGuidAsync(Guid referredUserGuid, CancellationToken cancellationToken = default);

    /// <summary>Idempotency check — whether a relationship for this referred user already exists.</summary>
    Task<bool> ExistsByReferredUserGuidAsync(Guid referredUserGuid, CancellationToken cancellationToken = default);
}

