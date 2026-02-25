namespace CryptoJackpot.Wallet.Domain.Enums;
public enum WalletTransactionDirection
{
    /// <summary>Money coming IN: commissions, prizes, bonuses.</summary>
    Credit = 1,
    /// <summary>Money going OUT: withdrawals or ticket purchases.</summary>
    Debit = 2,
}
