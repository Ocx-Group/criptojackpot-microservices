namespace CryptoJackpot.Wallet.Application.DTOs;

public class WithdrawalRequestDto
{
    public Guid RequestGuid { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
    public string WalletAddress { get; set; } = null!;
    public string CurrencySymbol { get; set; } = null!;
    public string CurrencyName { get; set; } = null!;
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
