using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Integration event published by Keycloak SPI when a new user registers.
/// This event is published directly to Kafka by the Keycloak Event Listener.
/// Consumed by: Identity microservice to auto-provision user in local database.
/// </summary>
public class KeycloakUserCreatedEvent : Event
{
    /// <summary>
    /// Keycloak user ID (UUID) - the subject claim in JWT tokens.
    /// </summary>
    public string KeycloakId { get; set; } = null!;
    
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// User's first name (optional, may not be set during registration).
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// User's last name (optional, may not be set during registration).
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Whether the user's email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }
    
    /// <summary>
    /// Custom attributes from Keycloak user profile.
    /// </summary>
    public Dictionary<string, string>? Attributes { get; set; }
}

