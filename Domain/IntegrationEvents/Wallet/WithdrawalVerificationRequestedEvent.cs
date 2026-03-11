using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;

/// <summary>
/// Integration event published when a withdrawal verification code is requested.
/// Consumed by: Notification microservice to send verification email.
/// </summary>
public class WithdrawalVerificationRequestedEvent : Event
{
    public Guid UserGuid { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string SecurityCode { get; set; } = null!;
    public decimal Amount { get; set; }
}
