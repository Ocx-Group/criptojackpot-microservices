namespace CryptoJackpot.Identity.Application.DTOs;

/// <summary>
/// DTO with 2FA status information for a user.
/// </summary>
public class TwoFactorStatusDto
{
    /// <summary>
    /// Whether 2FA is enabled for the user.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Whether 2FA setup has been initiated but not confirmed.
    /// </summary>
    public bool IsPendingSetup { get; set; }

    /// <summary>
    /// Number of unused recovery codes remaining.
    /// Only populated if 2FA is enabled.
    /// </summary>
    public int? RecoveryCodesRemaining { get; set; }
}

