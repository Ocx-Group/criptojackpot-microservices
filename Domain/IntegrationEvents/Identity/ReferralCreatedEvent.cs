using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Integration event published when a referral is created.
/// Consumed by: Notification microservice
/// </summary>
public class ReferralCreatedEvent : Event
{
    public string ReferrerEmail { get; set; } = null!;
    public string ReferrerName { get; set; } = null!;
    public string ReferrerLastName { get; set; } = null!;
    public string ReferredName { get; set; } = null!;
    public string ReferredLastName { get; set; } = null!;
    public string ReferralCode { get; set; } = null!;
}
