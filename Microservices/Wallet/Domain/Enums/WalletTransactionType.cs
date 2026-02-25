namespace CryptoJackpot.Wallet.Domain.Enums;
/// <summary>
/// Defines the nature and origin of each internal wallet movement.
/// </summary>
public enum WalletTransactionType
{
    /// <summary>Credit earned from a direct referral bonus when a referred user registers.</summary>
    ReferralBonus = 1,
    /// <summary>Credit earned as a commission when a referred user completes a ticket purchase.</summary>
    ReferralPurchaseCommission = 2,
    /// <summary>Credit earned when the user wins a lottery prize.</summary>
    LotteryPrize = 3,
    /// <summary>Debit generated when the user uses internal balance to purchase a lottery ticket.</summary>
    TicketPurchase = 4,
    /// <summary>Debit generated when the user requests a withdrawal to an external crypto wallet.</summary>
    Withdrawal = 5,
    /// <summary>Credit applied when a previously processed withdrawal is reversed or refunded.</summary>
    WithdrawalRefund = 6,
    /// <summary>Manual credit applied by platform administrators (e.g., promotions, corrections).</summary>
    AdminCredit = 7,
    /// <summary>Manual debit applied by platform administrators (e.g., chargebacks, corrections).</summary>
    AdminDebit = 8,
}
