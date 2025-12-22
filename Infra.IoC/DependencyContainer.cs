using CryptoJackpot.Domain.Core.Bus;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoJackpot.Infra.IoC;

public static class DependencyContainer
{
    /// <summary>
    /// Registers shared infrastructure services (EventBus, MassTransit).
    /// For microservices that DON'T need Kafka (simple setup).
    /// </summary>
    public static void RegisterServices(
        IServiceCollection services, 
        IConfiguration configuration)
    {
        // Domain Bus
        services.AddTransient<IEventBus, Bus.MassTransitBus>();

        // MassTransit with In-Memory bus only
        services.AddMassTransit(x =>
        {
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
    }

    /// <summary>
    /// Registers shared infrastructure services (EventBus, MassTransit, Kafka).
    /// For microservices that need Kafka consumers/producers.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">App configuration</param>
    /// <param name="configureKafka">Configure Kafka Rider (consumers, topics, producers)</param>
    public static void RegisterServicesWithKafka(
        IServiceCollection services, 
        IConfiguration configuration,
        Action<IRiderRegistrationConfigurator> configureKafka)
    {
        // Domain Bus
        services.AddTransient<IEventBus, Bus.MassTransitBus>();

        var kafkaHost = configuration["Kafka:Host"] ?? "localhost:9092";

        // MassTransit with Kafka
        services.AddMassTransit(x =>
        {
            // In-memory bus for internal messaging
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            // Kafka Rider
            x.AddRider(rider =>
            {
                // Let each microservice configure its consumers/producers/topics
                configureKafka(rider);

                // Configure Kafka host
                rider.UsingKafka((context, k) =>
                {
                    k.Host(kafkaHost);
                });
            });
        });
    }
}