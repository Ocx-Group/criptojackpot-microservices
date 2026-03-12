using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Identity;

/// <summary>
/// Integration event published when a user successfully logs out.
/// Consumed by: Audit microservice
/// </summary>
public class UserLoggedOutEvent : Event
{
    public long UserId { get; set; }
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public DateTime LogoutTime { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public UserLoggedOutEvent() { }

    public UserLoggedOutEvent(long userId, string email, string userName, string? ipAddress = null, string? userAgent = null)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        LogoutTime = DateTime.UtcNow;
    }
}

