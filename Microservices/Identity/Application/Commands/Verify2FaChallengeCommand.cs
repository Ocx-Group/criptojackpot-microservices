using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

/// <summary>
/// Command to verify 2FA code and complete login.
/// Accepts either TOTP code from authenticator app or recovery code.
/// </summary>
public class Verify2FaChallengeCommand : IRequest<Result<LoginResultDto>>
{
    /// <summary>
    /// Challenge token from initial login (stored in HttpOnly cookie).
    /// Contains "purpose": "2fa_challenge" claim.
    /// </summary>
    public string ChallengeToken { get; set; } = null!;

    /// <summary>
    /// 6-digit TOTP code from authenticator app.
    /// Either Code or RecoveryCode must be provided.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Recovery code (backup code) if user lost access to authenticator.
    /// Format: XXXX-XXXX (8 alphanumeric chars with hyphen).
    /// </summary>
    public string? RecoveryCode { get; set; }

    /// <summary>
    /// Client IP address for audit and refresh token.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Device info/User-Agent for session tracking.
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Remember me preference from original login.
    /// </summary>
    public bool RememberMe { get; set; }
}

