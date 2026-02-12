using CryptoJackpot.Domain.Core.Enums;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Configuration;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Implements refresh token rotation with reuse detection.
/// If a revoked token is reused, entire token family is revoked and security alert is published.
/// </summary>
public class TokenRotationService : ITokenRotationService
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly JwtConfig _jwtConfig;
    private readonly ILogger<TokenRotationService> _logger;

    public TokenRotationService(
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        IIdentityEventPublisher eventPublisher,
        IOptions<JwtConfig> jwtConfig,
        ILogger<TokenRotationService> logger)
    {
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _jwtConfig = jwtConfig.Value;
        _logger = logger;
    }

    public async Task<Result<TokenRotationResultDto>> RotateTokenAsync(
        string rawToken,
        string? deviceInfo,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var tokenHash = _refreshTokenService.HashToken(rawToken);
        var existingToken = await _refreshTokenRepository.GetByHashAsync(tokenHash);

        // Token not found
        if (existingToken is null)
        {
            _logger.LogWarning("Refresh token not found. Hash prefix: {HashPrefix}", 
                tokenHash[..8]);
            return Result.Fail(new UnauthorizedError("Invalid refresh token."));
        }

        // Token already revoked = REUSE ATTACK DETECTED
        if (existingToken.IsRevoked)
        {
            await HandleTokenReuseAsync(existingToken, ipAddress, cancellationToken);
            return Result.Fail(new UnauthorizedError("Token has been revoked. Please login again."));
        }

        // Token expired
        if (!existingToken.IsActive)
        {
            _logger.LogInformation("Expired refresh token used. UserId: {UserId}", 
                existingToken.UserId);
            return Result.Fail(new UnauthorizedError("Refresh token expired. Please login again."));
        }

        // Create new token pair (rotation)
        var (newRawToken, newTokenEntity) = _refreshTokenService.CreateRefreshToken(
            existingToken.UserId,
            familyId: existingToken.FamilyId, // Keep same family
            deviceInfo: deviceInfo ?? existingToken.DeviceInfo,
            ipAddress: ipAddress ?? existingToken.IpAddress,
            rememberMe: IsRememberMeToken(existingToken));

        // Revoke old token and link to new one
        existingToken.Revoke("rotated", newTokenEntity.TokenHash);

        // Generate new access token
        var accessToken = _jwtTokenService.GenerateAccessToken(existingToken.User);

        // Persist changes
        await _refreshTokenRepository.AddAsync(newTokenEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Token rotated successfully. UserId: {UserId}, FamilyId: {FamilyId}",
            existingToken.UserId, existingToken.FamilyId);

        return Result.Ok(new TokenRotationResultDto
        {
            AccessToken = accessToken,
            RefreshToken = newRawToken,
            ExpiresInMinutes = _jwtConfig.ExpirationInMinutes
        });
    }

    /// <summary>
    /// Handles token reuse: revokes entire family and publishes security alert.
    /// </summary>
    private async Task HandleTokenReuseAsync(
        UserRefreshToken revokedToken,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "TOKEN REUSE DETECTED! UserId: {UserId}, FamilyId: {FamilyId}, OriginalRevokeReason: {Reason}, AttackerIP: {IP}",
            revokedToken.UserId,
            revokedToken.FamilyId,
            revokedToken.RevokedReason,
            ipAddress);

        // Revoke ALL tokens in this family (nuclear option)
        await _refreshTokenRepository.RevokeByFamilyIdAsync(
            revokedToken.FamilyId,
            "reuse_detected");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish security alert for notification service
        await _eventPublisher.PublishSecurityAlertAsync(
            revokedToken.UserId,
            revokedToken.User.Email,
            SecurityAlertType.RefreshTokenReuse,
            ipAddress);
    }

    /// <summary>
    /// Determines if token was created with "remember me" based on expiration.
    /// </summary>
    private static bool IsRememberMeToken(UserRefreshToken token)
    {
        // RememberMe tokens have 30 day expiration vs 7 day standard
        var totalDays = (token.ExpiresAt - token.CreatedAt).TotalDays;
        return totalDays > 20; // If more than 20 days, it was a remember me token
    }
}

