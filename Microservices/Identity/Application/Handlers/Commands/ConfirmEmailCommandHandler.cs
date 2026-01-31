using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

/// <summary>
/// Syncs local user status with Keycloak's email verification status.
/// This endpoint is called by Keycloak after successful email verification
/// or can be used to manually sync status.
/// </summary>
public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IKeycloakAdminService _keycloakAdminService;

    public ConfirmEmailCommandHandler(
        IUserRepository userRepository,
        IKeycloakAdminService keycloakAdminService)
    {
        _userRepository = userRepository;
        _keycloakAdminService = keycloakAdminService;
    }

    public async Task<Result<string>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Fail<string>(new BadRequestError("Invalid token"));

        // Token is the KeycloakId - find user and sync status
        var user = await _userRepository.GetByKeycloakIdAsync(request.Token);

        if (user == null)
        {
            // Try by email if token is email
            user = await _userRepository.GetByEmailAsync(request.Token);
        }

        if (user == null || string.IsNullOrEmpty(user.KeycloakId))
            return Result.Fail<string>(new NotFoundError("User not found"));

        if (user.Status)
            return Result.Ok("Email already confirmed");

        // Check Keycloak for verification status
        var keycloakUser = await _keycloakAdminService.GetUserByIdAsync(user.KeycloakId, cancellationToken);
        if (keycloakUser == null)
            return Result.Fail<string>(new NotFoundError("User not found in authentication service"));

        if (!keycloakUser.EmailVerified)
            return Result.Fail<string>(new BadRequestError("Email not yet verified. Please check your email."));

        // Sync local status with Keycloak
        user.Status = true;
        await _userRepository.UpdateAsync(user);

        return Result.Ok("Email confirmed successfully");
    }
}
