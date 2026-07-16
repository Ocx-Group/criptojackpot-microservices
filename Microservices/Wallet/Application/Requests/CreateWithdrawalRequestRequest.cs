namespace CryptoJackpot.Wallet.Application.Requests;

public class CreateWithdrawalRequestRequest
{
    /// <summary>Amount in USD to withdraw.</summary>
    public decimal Amount { get; set; }

    /// <summary>Guid of the user's saved crypto wallet to withdraw to.</summary>
    public Guid WalletGuid { get; set; }

    /// <summary>TOTP code from authenticator app (required if 2FA is enabled).</summary>
    public string? TwoFactorCode { get; set; }

    /// <summary>Email verification code (required if 2FA is NOT enabled).</summary>
    public string? EmailVerificationCode { get; set; }
}
