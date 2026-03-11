using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;

/// <summary>
/// Integration event published by the Wallet service after successfully crediting
/// the referral signup bonus to the referrer's wallet.
/// Consumed by: Notification microservice to send a commission email.
/// </summary>
public class ReferralBonusCreditedEvent : Event
{
    public Guid ReferrerUserGuid { get; set; }
    public string ReferrerEmail { get; set; } = null!;
    public string ReferrerName { get; set; } = null!;
    public string ReferrerLastName { get; set; } = null!;
    public string ReferredName { get; set; } = null!;
    public string ReferredLastName { get; set; } = null!;
    public string ReferralCode { get; set; } = null!;
    public decimal BonusAmount { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid TransactionGuid { get; set; }
    public DateTime CreditedAt { get; set; }
}

