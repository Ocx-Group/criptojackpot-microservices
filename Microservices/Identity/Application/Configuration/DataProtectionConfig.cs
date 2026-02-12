namespace CryptoJackpot.Identity.Application.Configuration;

/// <summary>
/// Configuration for ASP.NET Core Data Protection.
/// Used to encrypt sensitive data at rest (TwoFactorSecret, Google tokens).
/// </summary>
public class DataProtectionConfig
{
    public const string SectionName = "DataProtection";

    /// <summary>
    /// Application name for key isolation.
    /// All instances must use the same name to share keys.
    /// </summary>
    public string ApplicationName { get; set; } = "CryptoJackpot.Identity";

    /// <summary>
    /// Redis connection string for key persistence in production.
    /// When null, keys are stored in the default location (file system in dev).
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Redis key name for storing Data Protection keys.
    /// </summary>
    public string RedisKeyName { get; set; } = "DataProtection-Keys";

    /// <summary>
    /// Key lifetime in days. After this period, new keys are generated.
    /// Default: 90 days.
    /// </summary>
    public int KeyLifetimeDays { get; set; } = 90;
}

