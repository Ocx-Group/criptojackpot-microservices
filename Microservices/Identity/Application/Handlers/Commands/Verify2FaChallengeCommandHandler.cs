using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

/// <summary>
/// Handles 2FA challenge verification and completes login.
/// </summary>
public class Verify2FaChallengeCommandHandler : IRequestHandler<Verify2FaChallengeCommand, Result<LoginResultDto>>
{
    private readonly ITwoFactorVerificationService _twoFactorService;

    public Verify2FaChallengeCommandHandler(ITwoFactorVerificationService twoFactorService)
    {
        _twoFactorService = twoFactorService;
    }

    public async Task<Result<LoginResultDto>> Handle(
        Verify2FaChallengeCommand request,
        CancellationToken cancellationToken)
    {
        return await _twoFactorService.VerifyChallengeAsync(
            request.ChallengeToken,
            request.Code,
            request.RecoveryCode,
            request.DeviceInfo,
            request.IpAddress,
            request.RememberMe,
            cancellationToken);
    }
}

