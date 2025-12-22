using CryptoJackpot.Domain.Core.Commands;
using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.Bus;

public interface IEventBus
{
    Task SendCommand<T>(T command) where T : Command;

    Task Publish<T>(T @event) where T : Event;

    void Subscribe<T, TH>()
        where T : Event
        where TH : IEventHandler<T>;
}
