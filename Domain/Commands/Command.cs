using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.Commands;

public abstract class Command : Message
{
    public DateTime Timestamp { get; protected set; }

    protected Command()
    {
        Timestamp = DateTime.UtcNow;
    }
}
