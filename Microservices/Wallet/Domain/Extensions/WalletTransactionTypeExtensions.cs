using CryptoJackpot.Wallet.Domain.Enums;

namespace CryptoJackpot.Wallet.Domain.Extensions;
public static class WalletTransactionTypeExtensions
{
    private static readonly IReadOnlySet<WalletTransactionType> CreditOnlyTypes =
        new HashSet<WalletTransactionType>
        {
            WalletTransactionType.ReferralBonus,
            WalletTransactionType.ReferralPurchaseCommission,
            WalletTransactionType.LotteryPrize,
            WalletTransactionType.WithdrawalRefund,
            WalletTransactionType.AdminCredit,
        };
    private static readonly IReadOnlySet<WalletTransactionType> DebitOnlyTypes =
        new HashSet<WalletTransactionType>
        {
            WalletTransactionType.Withdrawal,
            WalletTransactionType.TicketPurchase,
            WalletTransactionType.AdminDebit,
        };
    /// <summary>Returns true if this type can only ever be a Credit movement.</summary>
    public static bool IsCreditOnly(this WalletTransactionType type) => CreditOnlyTypes.Contains(type);
    /// <summary>Returns true if this type can only ever be a Debit movement.</summary>
    public static bool IsDebitOnly(this WalletTransactionType type) => DebitOnlyTypes.Contains(type);
    /// <summary>
    /// Returns true if the given direction is coherent with this transaction type.
    /// Example: ReferralBonus.IsCoherentWith(Debit) == false
    /// </summary>
    public static bool IsCoherentWith(this WalletTransactionType type, WalletTransactionDirection direction) =>
        direction switch
        {
            WalletTransactionDirection.Credit => !DebitOnlyTypes.Contains(type),
            WalletTransactionDirection.Debit  => !CreditOnlyTypes.Contains(type),
            _ => false,
        };
}
