using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Integration event published when a new user registers.
/// Consumed by: Notification microservice
/// </summary>
public class UserRegisteredEvent : Event
{
    /// <summary>
    /// Internal database ID
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// External GUID for cross-service communication
    /// </summary>
    public Guid UserGuid { get; set; }
    
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string ConfirmationToken { get; set; } = null!;
}
