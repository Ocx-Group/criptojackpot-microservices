using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

/// <summary>
/// Handler for Get2FaStatusQuery.
/// Returns 2FA status and recovery codes count.
/// </summary>
public class Get2FaStatusQueryHandler : IRequestHandler<Get2FaStatusQuery, Result<TwoFactorStatusDto>>
{
    private readonly IUserRepository _userRepository;

    public Get2FaStatusQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<TwoFactorStatusDto>> Handle(
        Get2FaStatusQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidWithRecoveryCodesAsync(request.UserGuid);
        if (user is null)
        {
            return Result.Fail(new NotFoundError("User not found."));
        }

        var status = new TwoFactorStatusDto
        {
            IsEnabled = user.TwoFactorEnabled,
            IsPendingSetup = !user.TwoFactorEnabled && !string.IsNullOrWhiteSpace(user.TwoFactorSecret),
            RecoveryCodesRemaining = user.TwoFactorEnabled 
                ? user.RecoveryCodes.Count(c => !c.IsUsed) 
                : null
        };

        return Result.Ok(status);
    }
}

