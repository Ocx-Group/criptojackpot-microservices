using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Application.Interfaces;

/// <summary>
/// Publishes identity-related events to the event bus.
/// Note: Email verification and password reset emails are now handled by Keycloak.
/// </summary>
public interface IIdentityEventPublisher
{
    Task PublishReferralCreatedAsync(User referrer, User referred, string referralCode);
    Task PublishUserLoggedInAsync(User user);
}
