using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Infra.Bus;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoJackpot.Infra.IoC;

public static class DependencyContainer
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, Action<IBusRegistrationConfigurator>? massTransitConfig = null)
    {
        // Domain Bus
        services.AddTransient<IEventBus, CryptoJackpot.Infra.Bus.MassTransitBus>();

        // MassTransit Base Config
        services.AddMassTransit(x =>
        {
            // Permitir configuración extra desde cada microservicio (ej. Consumers)
            massTransitConfig?.Invoke(x);

            // Bus de control en memoria (necesario para orquestar los Riders)
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            // Configuración de Kafka
            x.AddRider(rider =>
            {
                // Aquí registraremos los Productores/Consumidores de Kafka
                // Cada microservicio puede agregar los suyos mediante extensiones si fuera necesario,
                // pero la configuración del Host es común.
                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:Host"] ?? "localhost:9092");
                });
            });
        });
    }
}
