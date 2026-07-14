namespace CryptoJackpot.Lottery.Domain.Models;

/// <summary>
/// Language codes accepted as translation keys. Spanish is the base language
/// stored in the entities' plain fields, so it is not a valid override key.
/// </summary>
public static class TranslationLanguages
{
    public static readonly string[] Supported = ["en", "pt"];
}
