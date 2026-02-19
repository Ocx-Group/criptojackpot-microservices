namespace CryptoJackpot.Identity.Application.Requests;

/// <summary>
/// Request to regenerate 2FA recovery codes.
/// </summary>
public class Regenerate2FaRecoveryCodesRequest
{
    /// <summary>
    /// Current 6-digit TOTP code from authenticator app.
    /// Required to verify user has access to authenticator.
    /// </summary>
    public string Code { get; set; } = null!;
}

