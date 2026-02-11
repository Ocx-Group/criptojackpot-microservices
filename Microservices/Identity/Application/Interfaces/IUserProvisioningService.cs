using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Service for provisioning users from Keycloak into the local database.
/// Centralizes user creation logic to avoid duplication between sync query and Kafka consumer.
/// </summary>
public interface IUserProvisioningService
{
    /// <summary>
    /// Provisions a user from Keycloak into the local database.
    /// If user already exists (by KeycloakId or Email), links and returns existing user.
    /// Otherwise, creates a new user with data from Keycloak.
    /// </summary>
    /// <param name="keycloakId">Keycloak user ID (UUID)</param>
    /// <param name="email">User's email address</param>
    /// <param name="firstName">User's first name (optional)</param>
    /// <param name="lastName">User's last name (optional)</param>
    /// <param name="emailVerified">Whether email is verified</param>
    /// <param name="attributes">Custom attributes from Keycloak</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result with provisioned user DTO</returns>
    Task<Result<UserDto>> ProvisionUserAsync(
        string keycloakId,
        string email,
        string? firstName = null,
        string? lastName = null,
        bool emailVerified = false,
        Dictionary<string, string>? attributes = null,
        CancellationToken cancellationToken = default);
}

