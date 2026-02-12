using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

/// <summary>
/// Handles refresh token rotation with reuse detection.
/// Critical security flow: detects stolen tokens and revokes entire family.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenRotationResultDto>>
{
    private readonly ITokenRotationService _tokenRotationService;

    public RefreshTokenCommandHandler(ITokenRotationService tokenRotationService)
    {
        _tokenRotationService = tokenRotationService;
    }

    public async Task<Result<TokenRotationResultDto>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        return await _tokenRotationService.RotateTokenAsync(
            request.RefreshToken,
            request.DeviceInfo,
            request.IpAddress,
            cancellationToken);
    }
}

