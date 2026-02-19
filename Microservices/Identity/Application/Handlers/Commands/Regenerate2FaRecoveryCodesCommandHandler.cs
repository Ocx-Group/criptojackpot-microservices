using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

/// <summary>
/// Handler for regenerating 2FA recovery codes.
/// </summary>
public class Regenerate2FaRecoveryCodesCommandHandler 
    : IRequestHandler<Regenerate2FaRecoveryCodesCommand, Result<Confirm2FaResultDto>>
{
    private readonly ITwoFactorSetupService _twoFactorSetupService;

    public Regenerate2FaRecoveryCodesCommandHandler(ITwoFactorSetupService twoFactorSetupService)
    {
        _twoFactorSetupService = twoFactorSetupService;
    }

    public async Task<Result<Confirm2FaResultDto>> Handle(
        Regenerate2FaRecoveryCodesCommand request,
        CancellationToken cancellationToken)
    {
        return await _twoFactorSetupService.RegenerateRecoveryCodesAsync(
            request.UserGuid,
            request.Code,
            cancellationToken);
    }
}

