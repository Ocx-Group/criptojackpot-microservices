using CryptoJackpot.Domain.Core.Bus;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoJackpot.Infra.IoC;

public static class DependencyContainer
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? massTransitConfig = null)
    {
        // Domain Bus
        services.AddTransient<IEventBus, Bus.MassTransitBus>();

        // MassTransit Base Config
        services.AddMassTransit(x =>
        {
            // Permitir configuración extra desde cada microservicio (ej. Consumers)
            massTransitConfig?.Invoke(x);

            // Bus de control en memoria
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            // Configuración de Kafka
            x.AddRider(rider =>
            {
                // Aquí registramos los Productores/Consumidores de Kafka
                // pero la configuración del Host es común.
                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:Host"] ?? "localhost:9092");
                });
            });
        });
    }
}