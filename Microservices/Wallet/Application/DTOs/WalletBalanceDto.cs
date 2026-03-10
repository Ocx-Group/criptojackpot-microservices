namespace CryptoJackpot.Wallet.Application.DTOs;

public class WalletBalanceDto
{
    public decimal Balance { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public decimal TotalPurchased { get; set; }
}
