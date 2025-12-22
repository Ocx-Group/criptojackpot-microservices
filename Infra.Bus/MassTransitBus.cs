using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.Commands;
using MassTransit;
using CoreEvent = CryptoJackpot.Domain.Core.Events.Event;

namespace CryptoJackpot.Infra.Bus;

public class MassTransitBus(IPublishEndpoint publishEndpoint, 
    ISendEndpointProvider sendEndpointProvider) : IEventBus
{
    private readonly ISendEndpointProvider _sendEndpointProvider = sendEndpointProvider;

    public Task SendCommand<T>(T command) where T : Command
    {
        return publishEndpoint.Publish(command);
    }

    public Task Publish<T>(T @event) where T : CoreEvent
    {
        return publishEndpoint.Publish(@event);
    }

    public void Subscribe<T, TH>()
        where T : CoreEvent
        where TH : IEventHandler<T>
    {
        throw new NotImplementedException("Con MassTransit, registra los consumidores (Consumers) en la capa IoC usando AddMassTransit.");
    }
}