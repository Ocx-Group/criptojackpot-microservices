using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Identity.Application.Events;

/// <summary>
/// Event published via IEventBus when a user successfully logs in.
/// Other microservices can subscribe to this event.
/// </summary>
public class UserLoggedInEvent : Event
{
    public long UserId { get; }
    public string Email { get; }
    public string UserName { get; }
    public DateTime LoginTime { get; }

    public UserLoggedInEvent(long userId, string email, string userName)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
        LoginTime = DateTime.UtcNow;
    }
}
