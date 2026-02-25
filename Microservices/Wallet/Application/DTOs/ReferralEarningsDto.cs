namespace CryptoJackpot.Wallet.Application.DTOs;

public class ReferralEarningsDto
{
    /// <summary>Total referral earnings (all time).</summary>
    public decimal TotalEarnings { get; set; }

    /// <summary>Referral earnings in the last 30 days.</summary>
    public decimal LastMonthEarnings { get; set; }
}

