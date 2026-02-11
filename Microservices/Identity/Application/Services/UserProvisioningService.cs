using AutoMapper;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Application.Models;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Service for provisioning users from Keycloak into the local database.
/// Centralizes user creation logic to avoid duplication between sync query and Kafka consumer.
/// </summary>
public class UserProvisioningService : IUserProvisioningService
{
    private readonly IUserRepository _userRepository;
    private readonly IKeycloakUserService _keycloakUserService;
    private readonly IStorageService _storageService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserProvisioningService> _logger;

    /// <summary>
    /// Default role ID for self-registered users (client/user role).
    /// </summary>
    private const long DefaultUserRoleId = 2;

    public UserProvisioningService(
        IUserRepository userRepository,
        IKeycloakUserService keycloakUserService,
        IStorageService storageService,
        IMapper mapper,
        ILogger<UserProvisioningService> logger)
    {
        _userRepository = userRepository;
        _keycloakUserService = keycloakUserService;
        _storageService = storageService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserDto>> ProvisionUserAsync(
        string keycloakId,
        string email,
        string? firstName = null,
        string? lastName = null,
        bool emailVerified = false,
        Dictionary<string, string>? attributes = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Check if user already exists by KeycloakId
        var existingUser = await _userRepository.GetByKeycloakIdAsync(keycloakId);
        if (existingUser is not null)
        {
            _logger.LogDebug(
                "User already exists with KeycloakId {KeycloakId}. UserId: {UserId}",
                keycloakId, existingUser.Id);
            return Result.Ok(MapToDto(existingUser));
        }

        // 2. Check if user exists by email (created by admin without KeycloakId)
        existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser is not null)
        {
            // Link existing user to Keycloak
            existingUser.KeycloakId = keycloakId;
            await _userRepository.UpdateAsync(existingUser);
            await SetUserIdAttributeInKeycloak(keycloakId, existingUser.Id, cancellationToken);

            _logger.LogInformation(
                "Linked existing user {UserId} to Keycloak ID {KeycloakId}",
                existingUser.Id, keycloakId);

            return Result.Ok(MapToDto(existingUser));
        }

        // 3. Auto-provision: create new user from Keycloak data
        _logger.LogInformation(
            "Auto-provisioning user from Keycloak. KeycloakId={KeycloakId}, Email={Email}",
            keycloakId, email);

        // Try to fetch additional profile data from Keycloak
        var keycloakUser = await _keycloakUserService.GetUserByIdAsync(keycloakId, cancellationToken);

        var countryId = GetCountryId(keycloakUser, attributes);
        var statePlace = GetAttributeValue(keycloakUser, attributes, "statePlace") ?? "";
        var city = GetAttributeValue(keycloakUser, attributes, "city") ?? "";

        var newUser = new User
        {
            KeycloakId = keycloakId,
            Name = keycloakUser?.FirstName ?? firstName ?? "",
            LastName = keycloakUser?.LastName ?? lastName ?? "",
            Email = email,
            Phone = GetAttributeValue(keycloakUser, attributes, "phone"),
            CountryId = countryId,
            StatePlace = statePlace,
            City = city,
            Address = GetAttributeValue(keycloakUser, attributes, "address"),
            Status = emailVerified || (keycloakUser?.EmailVerified ?? false),
            RoleId = DefaultUserRoleId,
        };

        var createdUser = await _userRepository.CreateAsync(newUser);

        // 4. Set user_id attribute in Keycloak so future tokens include it
        await SetUserIdAttributeInKeycloak(keycloakId, createdUser.Id, cancellationToken);

        _logger.LogInformation(
            "Auto-provisioned user {UserId} from Keycloak ID {KeycloakId}",
            createdUser.Id, keycloakId);

        // Reload with navigation properties
        var reloadedUser = await _userRepository.GetByIdAsync(createdUser.Id);
        return Result.Ok(MapToDto(reloadedUser ?? createdUser));
    }

    private UserDto MapToDto(User user)
    {
        var dto = _mapper.Map<UserDto>(user);
        if (!string.IsNullOrEmpty(dto.ImagePath))
            dto.ImagePath = _storageService.GetPresignedUrl(dto.ImagePath);
        return dto;
    }

    private async Task SetUserIdAttributeInKeycloak(
        string keycloakId, 
        long userId, 
        CancellationToken cancellationToken)
    {
        try
        {
            var attributes = new Dictionary<string, List<string>>
            {
                ["user_id"] = [userId.ToString()]
            };
            await _keycloakUserService.UpdateUserAsync(
                keycloakId, attributes: attributes, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            // Non-fatal: user was created, attribute sync can be retried
            _logger.LogWarning(ex,
                "Failed to set user_id attribute in Keycloak for user {UserId}, KeycloakId {KeycloakId}",
                userId, keycloakId);
        }
    }

    private static long GetCountryId(KeycloakUserDto? keycloakUser, Dictionary<string, string>? attributes)
    {
        var countryValue = GetAttributeValue(keycloakUser, attributes, "country");
        if (!string.IsNullOrEmpty(countryValue) && long.TryParse(countryValue, out var countryId))
            return countryId;

        // Default country ID (fallback)
        return 1;
    }

    private static string? GetAttributeValue(
        KeycloakUserDto? keycloakUser, 
        Dictionary<string, string>? eventAttributes,
        string attributeName)
    {
        // First try from event attributes (passed from Keycloak SPI)
        if (eventAttributes?.TryGetValue(attributeName, out var eventValue) == true)
            return eventValue;
        
        // Fallback to Keycloak API response
        if (keycloakUser?.Attributes is null)
            return null;

        return keycloakUser.Attributes.TryGetValue(attributeName, out var values)
            ? values.FirstOrDefault()
            : null;
    }
}

