using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Wallet.Domain.Models;

/// <summary>
/// Local projection of referral relationships consumed from Identity service.
/// Maps a referred user to their referrer, enabling referral purchase commission calculations.
/// Populated when <see cref="CryptoJackpot.Domain.Core.IntegrationEvents.Identity.ReferralCreatedEvent"/> is consumed.
/// </summary>
public class ReferralRelationship : BaseEntity
{
    /// <summary>UserGuid of the person who referred someone.</summary>
    public Guid ReferrerUserGuid { get; set; }

    /// <summary>UserGuid of the person who was referred.</summary>
    public Guid ReferredUserGuid { get; set; }

    /// <summary>Referral code used at registration (snapshot for audit).</summary>
    public string ReferralCode { get; set; } = null!;
}

