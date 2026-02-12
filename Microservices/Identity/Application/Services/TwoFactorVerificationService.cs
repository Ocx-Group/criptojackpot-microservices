using CryptoJackpot.Domain.Core.Enums;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Service for verifying 2FA challenges during login.
/// Supports TOTP codes and recovery codes.
/// </summary>
public class TwoFactorVerificationService : ITwoFactorVerificationService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICodeVerificationService _codeVerificationService;
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TwoFactorVerificationService> _logger;

    public TwoFactorVerificationService(
        IJwtTokenService jwtTokenService,
        ICodeVerificationService codeVerificationService,
        IUserRepository userRepository,
        IAuthenticationService authenticationService,
        IIdentityEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        ILogger<TwoFactorVerificationService> logger)
    {
        _jwtTokenService = jwtTokenService;
        _codeVerificationService = codeVerificationService;
        _userRepository = userRepository;
        _authenticationService = authenticationService;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginResultDto>> VerifyChallengeAsync(
        string challengeToken,
        string? totpCode,
        string? recoveryCode,
        string? deviceInfo,
        string? ipAddress,
        bool rememberMe,
        CancellationToken cancellationToken)
    {
        // Step 1: Validate challenge token
        var userGuid = _jwtTokenService.ValidateTwoFactorChallengeToken(challengeToken);
        if (userGuid is null)
        {
            _logger.LogWarning("Invalid or expired 2FA challenge token");
            return Result.Fail(new UnauthorizedError("Challenge token is invalid or expired."));
        }

        // Step 2: Get user with recovery codes
        var user = await _userRepository.GetByGuidWithRecoveryCodesAsync(userGuid.Value);
        if (user is null)
        {
            _logger.LogWarning("User not found for 2FA challenge. UserGuid: {UserGuid}", userGuid);
            return Result.Fail(new UnauthorizedError("User not found."));
        }

        // Step 3: Check if user is locked out
        if (user.IsLockedOut)
        {
            var lockoutMinutes = _authenticationService.GetLockoutMinutes(user.FailedLoginAttempts);
            return Result.Fail(new LockedError(
                $"Account is locked. Try again in {lockoutMinutes} minutes.",
                lockoutMinutes * 60));
        }

        // Step 4: Verify code (TOTP or recovery)
        var verificationResult = _codeVerificationService.VerifyCode(user, totpCode, recoveryCode);
        if (verificationResult.IsFailed)
        {
            await HandleFailedVerificationAsync(user, ipAddress, deviceInfo, cancellationToken);
            return Result.Fail(verificationResult.Errors);
        }

        // Step 5: Complete login (NOW we can call RegisterSuccessfulLogin)
        return await _authenticationService.CompleteLoginAsync(
            user,
            deviceInfo,
            ipAddress,
            rememberMe,
            cancellationToken);
    }

    private async Task HandleFailedVerificationAsync(
        Domain.Models.User user,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        user.RegisterFailedLogin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Check if lockout threshold reached
        if (user.IsLockedOut)
        {
            var lockoutMinutes = _authenticationService.GetLockoutMinutes(user.FailedLoginAttempts);
            await _eventPublisher.PublishUserLockedOutAsync(user, lockoutMinutes, ipAddress, userAgent);
        }

        // Publish security alert for brute force on 2FA
        if (user.FailedLoginAttempts >= 3)
        {
            await _eventPublisher.PublishSecurityAlertAsync(
                user,
                SecurityAlertType.TwoFactorBruteForce,
                $"Multiple failed 2FA attempts: {user.FailedLoginAttempts}",
                ipAddress,
                userAgent);
        }
    }
}
