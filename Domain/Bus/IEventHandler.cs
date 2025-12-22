using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.Bus;

public interface IEventHandler<in TEvent> : IEventHandler 
    where TEvent : Event
{
    Task Handle(TEvent @event);
}

public interface IEventHandler { }
