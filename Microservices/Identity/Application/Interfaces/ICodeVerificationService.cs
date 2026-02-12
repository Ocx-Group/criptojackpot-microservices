using CryptoJackpot.Identity.Domain.Models;
using FluentResults;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for verifying 2FA codes (TOTP or recovery).
/// Encapsulates code verification logic.
/// </summary>
public interface ICodeVerificationService
{
    /// <summary>
    /// Verifies a TOTP or recovery code for a user.
    /// </summary>
    /// <param name="user">User with TwoFactorSecret and RecoveryCodes loaded</param>
    /// <param name="totpCode">6-digit TOTP code (optional)</param>
    /// <param name="recoveryCode">Recovery code (optional)</param>
    /// <returns>Success with optional message, or failure with error</returns>
    Result<string?> VerifyCode(User user, string? totpCode, string? recoveryCode);
}

