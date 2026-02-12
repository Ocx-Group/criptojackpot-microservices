using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for refresh token rotation with reuse detection.
/// Implements the security-critical token rotation flow.
/// </summary>
public interface ITokenRotationService
{
    /// <summary>
    /// Rotates a refresh token: validates, revokes old token, issues new pair.
    /// Detects reuse attacks and revokes entire token family if detected.
    /// </summary>
    /// <param name="rawToken">The current refresh token from client</param>
    /// <param name="deviceInfo">Device fingerprint or User-Agent</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New access token and refresh token, or error if invalid/reused</returns>
    Task<Result<TokenRotationResultDto>> RotateTokenAsync(
        string rawToken,
        string? deviceInfo,
        string? ipAddress,
        CancellationToken cancellationToken);
}

