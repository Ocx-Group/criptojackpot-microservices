namespace CryptoJackpot.Identity.Application.DTOs;

/// <summary>
/// Result of refresh token rotation operation.
/// </summary>
public class TokenRotationResultDto
{
    /// <summary>
    /// New JWT access token (15 min TTL).
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// New refresh token (replaces the old one).
    /// </summary>
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// Access token expiration in minutes.
    /// </summary>
    public int ExpiresInMinutes { get; set; }

    /// <summary>
    /// Whether the original login was with "remember me" enabled.
    /// Used to set correct cookie expiration.
    /// </summary>
    public bool IsRememberMe { get; set; }
}

