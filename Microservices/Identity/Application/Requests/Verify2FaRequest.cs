namespace CryptoJackpot.Identity.Application.Requests;

/// <summary>
/// Request to verify 2FA code and complete login.
/// </summary>
public class Verify2FaRequest
{
    /// <summary>
    /// 6-digit TOTP code from authenticator app.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Recovery code if TOTP is unavailable (format: XXXX-XXXX).
    /// </summary>
    public string? RecoveryCode { get; set; }

    /// <summary>
    /// Remember me preference from original login.
    /// </summary>
    public bool RememberMe { get; set; }
}

