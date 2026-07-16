namespace CryptoJackpot.Lottery.Domain.Models;

/// <summary>
/// Localized texts for a prize, stored as jsonb keyed by language code
/// (e.g. "en", "pt"). The entity's base fields (Name/Description) are the
/// default language (Spanish); missing translations fall back to them.
/// </summary>
public class PrizeTranslation
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
