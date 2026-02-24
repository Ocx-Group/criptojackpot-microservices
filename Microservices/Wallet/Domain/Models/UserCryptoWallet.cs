using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Wallet.Domain.Models;

public class UserCryptoWallet : BaseEntity
{
    public Guid WalletGuid { get; set; } = Guid.NewGuid();
    public Guid UserGuid { get; set; }
    public string Address { get; set; } = null!;
    public string CurrencySymbol { get; set; } = null!;
    public string CurrencyName { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string Label { get; set; } = null!;
    public bool IsDefault { get; set; }
}
