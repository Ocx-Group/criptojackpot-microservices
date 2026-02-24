namespace CryptoJackpot.Wallet.Application.DTOs;

public class UserCryptoWalletDto
{
    public Guid WalletGuid { get; set; }
    public string Address { get; set; } = null!;
    public string CurrencySymbol { get; set; } = null!;
    public string CurrencyName { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string Label { get; set; } = null!;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}
