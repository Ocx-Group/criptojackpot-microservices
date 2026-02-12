using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Service for verifying 2FA codes (TOTP or recovery).
/// Handles decryption of TwoFactorSecret before validation.
/// </summary>
public class CodeVerificationService : ICodeVerificationService
{
    private readonly ITotpService _totpService;
    private readonly IRecoveryCodeService _recoveryCodeService;
    private readonly IDataEncryptionService _encryptionService;
    private readonly ILogger<CodeVerificationService> _logger;

    public CodeVerificationService(
        ITotpService totpService,
        IRecoveryCodeService recoveryCodeService,
        IDataEncryptionService encryptionService,
        ILogger<CodeVerificationService> logger)
    {
        _totpService = totpService;
        _recoveryCodeService = recoveryCodeService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public Result<string?> VerifyCode(User user, string? totpCode, string? recoveryCode)
    {
        // Try TOTP code first
        if (!string.IsNullOrWhiteSpace(totpCode))
        {
            return VerifyTotpCode(user, totpCode);
        }

        // Try recovery code
        if (!string.IsNullOrWhiteSpace(recoveryCode))
        {
            return VerifyRecoveryCode(user, recoveryCode);
        }

        return Result.Fail(new UnauthorizedError("Verification code is required."));
    }

    private Result<string?> VerifyTotpCode(User user, string totpCode)
    {
        if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
        {
            _logger.LogWarning("User {UserId} has 2FA enabled but no secret configured", user.Id);
            return Result.Fail(new UnauthorizedError("2FA is not properly configured."));
        }

        // Decrypt the secret before validation
        var decryptedSecret = _encryptionService.Decrypt(user.TwoFactorSecret);
        if (string.IsNullOrWhiteSpace(decryptedSecret))
        {
            _logger.LogError("Failed to decrypt TwoFactorSecret for user {UserId}", user.Id);
            return Result.Fail(new UnauthorizedError("2FA configuration error. Please contact support."));
        }

        if (_totpService.ValidateCode(decryptedSecret, totpCode))
        {
            _logger.LogInformation("2FA TOTP verification successful for user {UserId}", user.Id);
            return Result.Ok<string?>(null);
        }

        _logger.LogWarning("Invalid TOTP code for user {UserId}", user.Id);
        return Result.Fail(new UnauthorizedError("Invalid verification code."));
    }

    private Result<string?> VerifyRecoveryCode(User user, string recoveryCode)
    {
        var matchingCode = _recoveryCodeService.ValidateCode(recoveryCode, user.RecoveryCodes);
        if (matchingCode is not null)
        {
            matchingCode.MarkAsUsed();
            // sets IsUsed = true, so Count already excludes this code
            var remainingCount = user.RecoveryCodes.Count(c => !c.IsUsed);
            _logger.LogInformation(
                "2FA recovery code used for user {UserId}. Remaining codes: {RemainingCount}",
                user.Id,
                remainingCount);
            
            return Result.Ok<string?>($"Recovery code accepted. {remainingCount} codes remaining.");
        }

        _logger.LogWarning("Invalid recovery code for user {UserId}", user.Id);
        return Result.Fail(new UnauthorizedError("Invalid recovery code."));
    }
}

