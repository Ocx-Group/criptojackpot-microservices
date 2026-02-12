using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for Two-Factor Authentication verification during login.
/// </summary>
public interface ITwoFactorVerificationService
{
    /// <summary>
    /// Verifies 2FA challenge and completes login if successful.
    /// Supports both TOTP code and recovery code.
    /// </summary>
    /// <param name="challengeToken">JWT challenge token from initial login</param>
    /// <param name="totpCode">6-digit TOTP code (optional)</param>
    /// <param name="recoveryCode">Backup recovery code (optional)</param>
    /// <param name="deviceInfo">Device fingerprint</param>
    /// <param name="ipAddress">Client IP</param>
    /// <param name="rememberMe">Remember me preference</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login result with tokens on success</returns>
    Task<Result<LoginResultDto>> VerifyChallengeAsync(
        string challengeToken,
        string? totpCode,
        string? recoveryCode,
        string? deviceInfo,
        string? ipAddress,
        bool rememberMe,
        CancellationToken cancellationToken);
}

