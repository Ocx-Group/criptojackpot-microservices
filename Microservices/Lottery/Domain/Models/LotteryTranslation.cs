namespace CryptoJackpot.Lottery.Domain.Models;

/// <summary>
/// Localized texts for a lottery draw, stored as jsonb keyed by language code
/// (e.g. "en", "pt"). The entity's base fields (Title/Description/Terms) are the
/// default language (Spanish); missing translations fall back to them.
/// </summary>
public class LotteryTranslation
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Terms { get; set; }
}
