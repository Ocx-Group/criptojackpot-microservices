namespace CryptoJackpot.Wallet.Application.Requests;

public class CreateUserCryptoWalletRequest
{
    public string Address { get; set; } = null!;
    public string CurrencySymbol { get; set; } = null!;
    public string CurrencyName { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string Label { get; set; } = null!;
}
