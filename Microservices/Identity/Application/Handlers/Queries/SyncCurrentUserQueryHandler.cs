using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Application.Queries;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

/// <summary>
/// Handles SyncCurrentUserQuery: finds user by KeycloakId or auto-provisions
/// from Keycloak profile for self-registered users.
/// Delegates provisioning logic to IUserProvisioningService for reusability.
/// </summary>
public class SyncCurrentUserQueryHandler : IRequestHandler<SyncCurrentUserQuery, Result<UserDto>>
{
    private readonly IUserProvisioningService _provisioningService;
    private readonly ILogger<SyncCurrentUserQueryHandler> _logger;

    public SyncCurrentUserQueryHandler(
        IUserProvisioningService provisioningService,
        ILogger<SyncCurrentUserQueryHandler> logger)
    {
        _provisioningService = provisioningService;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(SyncCurrentUserQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Syncing current user. KeycloakId: {KeycloakId}, Email: {Email}",
            request.KeycloakId, request.Email);

        return await _provisioningService.ProvisionUserAsync(
            keycloakId: request.KeycloakId,
            email: request.Email,
            firstName: request.FirstName,
            lastName: request.LastName,
            emailVerified: request.EmailVerified,
            cancellationToken: cancellationToken);
    }
}
