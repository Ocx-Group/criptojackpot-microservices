namespace CryptoJackpot.Order.Application.DTOs;

public class CoinPaymentCurrencyDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public int DecimalPlaces { get; set; }
    public int Rank { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
    public int RequiredConfirmations { get; set; }
    public bool IsEnabledForPayment { get; set; }
}
