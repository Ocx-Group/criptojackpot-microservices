using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Identity.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Consumers;

/// <summary>
/// Consumes KeycloakUserCreatedEvent from Kafka (published by Keycloak SPI).
/// Auto-provisions the user in the local database immediately upon registration.
/// </summary>
public class KeycloakUserCreatedConsumer : IConsumer<KeycloakUserCreatedEvent>
{
    private readonly IUserProvisioningService _provisioningService;
    private readonly ILogger<KeycloakUserCreatedConsumer> _logger;

    public KeycloakUserCreatedConsumer(
        IUserProvisioningService provisioningService,
        ILogger<KeycloakUserCreatedConsumer> logger)
    {
        _provisioningService = provisioningService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<KeycloakUserCreatedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Received KeycloakUserCreatedEvent. KeycloakId: {KeycloakId}, Email: {Email}, CorrelationId: {CorrelationId}",
            message.KeycloakId, message.Email, message.CorrelationId);

        try
        {
            var result = await _provisioningService.ProvisionUserAsync(
                keycloakId: message.KeycloakId,
                email: message.Email,
                firstName: message.FirstName,
                lastName: message.LastName,
                emailVerified: message.EmailVerified,
                attributes: message.Attributes,
                cancellationToken: context.CancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Successfully provisioned user from Keycloak. UserGuid: {UserGuid}, KeycloakId: {KeycloakId}",
                    result.Value.UserGuid, message.KeycloakId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to provision user from Keycloak. KeycloakId: {KeycloakId}, Errors: {Errors}",
                    message.KeycloakId, string.Join(", ", result.Errors.Select(e => e.Message)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing KeycloakUserCreatedEvent. KeycloakId: {KeycloakId}, Email: {Email}",
                message.KeycloakId, message.Email);
            throw; // Rethrow to trigger retry mechanism
        }
    }
}

