namespace CryptoJackpot.Wallet.Domain.Interfaces;

/// <summary>
/// Client interface for querying referral data from Identity service via gRPC.
/// Identity is the single source of truth for referral relationships.
/// </summary>
public interface IReferralGrpcClient
{
    /// <summary>
    /// Resolves the referrer's UserGuid for a given referred user.
    /// Returns null if the user was not referred by anyone.
    /// </summary>
    Task<Guid?> GetReferrerUserGuidAsync(Guid referredUserGuid, CancellationToken cancellationToken = default);
}

