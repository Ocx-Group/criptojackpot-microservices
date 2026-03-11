using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;

/// <summary>
/// Integration event published by the Wallet service after successfully crediting
/// a 1% referral purchase commission to the referrer's wallet.
/// Consumed by: Notification microservice to send a commission email.
/// </summary>
public class ReferralCommissionCreditedEvent : Event
{
    public Guid ReferrerUserGuid { get; set; }
    public string ReferrerEmail { get; set; } = null!;
    public string ReferrerName { get; set; } = null!;
    public string ReferrerLastName { get; set; } = null!;
    public string BuyerName { get; set; } = null!;
    public string LotteryTitle { get; set; } = null!;
    public decimal CommissionAmount { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid TransactionGuid { get; set; }
    public Guid OrderId { get; set; }
    public DateTime CreditedAt { get; set; }
}

