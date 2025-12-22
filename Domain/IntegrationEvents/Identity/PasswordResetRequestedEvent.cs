using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Integration event published when a password reset is requested.
/// Consumed by: Notification microservice
/// </summary>
public class PasswordResetRequestedEvent : Event
{
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string SecurityCode { get; set; } = null!;
}
