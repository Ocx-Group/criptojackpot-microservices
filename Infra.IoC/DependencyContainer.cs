using CryptoJackpot.Domain.Core.Bus;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

namespace CryptoJackpot.Infra.IoC;

/// <summary>
/// Provides methods for registering services in the dependency injection container
/// with support for Kafka and MassTransit configurations.
/// </summary>
public static class DependencyContainer
{
    /// <summary>
    /// Registers OpenTelemetry tracing, metrics and logging with OTLP export.
    /// Configuration is read from the "OpenTelemetry" section:
    ///   Enabled  - bool (default true)
    ///   Endpoint - OTLP gRPC endpoint (default http://localhost:4317)
    /// </summary>
    public static void RegisterOpenTelemetry(
        IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<TracerProviderBuilder>? configureTracing = null)
    {
        var otelSection = configuration.GetSection("OpenTelemetry");
        var enabled = otelSection.GetValue("Enabled", true);
        if (!enabled) return;

        var endpoint = otelSection["Endpoint"] ?? "http://localhost:4317";
        var serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0";
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

        void ConfigureOtlpExporter(OtlpExporterOptions opts)
        {
            opts.Endpoint = new Uri(endpoint);
            opts.Protocol = OtlpExportProtocol.Grpc;
        }

        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                    })
                    .AddHttpClientInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(opts =>
                    {
                        opts.SetDbStatementForText = true;
                        opts.SetDbStatementForStoredProcedure = true;
                    })
                    .AddSource("MassTransit")
                    .AddOtlpExporter(ConfigureOtlpExporter);

                // Allow each microservice to add extra sources (e.g. MongoDB)
                configureTracing?.Invoke(tracing);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(ConfigureOtlpExporter);
            });
    }

    /// <summary>
    /// Registers shared infrastructure with Kafka and Transactional Outbox.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type for the microservice.</typeparam>
    /// <param name="services">The service collection used for dependency injection.</param>
    /// <param name="configuration">The application's configuration object.</param>
    /// <param name="configureRider">
    /// Optional action to configure the rider, allowing the registration of producers and consumers.
    /// </param>
    /// <param name="configureBus">
    /// Optional action to configure the bus, allowing the registration of consumers.
    /// </param>
    /// <param name="configureKafkaEndpoints">
    /// Optional action to configure Kafka topic endpoints, enabling consumers to subscribe to specific topics.
    /// </param>
    /// <param name="useMessageScheduler">
    /// Indicates whether to enable the in-memory message scheduler for handling delayed messages.
    /// </param>
    /// <remarks>
    /// This method integrates Kafka with MassTransit, enabling transactional outbox pattern support. It also
    /// provides configuration capabilities for producers, consumers, and Kafka endpoints per microservice.
    /// </remarks>
    public static void RegisterServicesWithKafka<TDbContext>(
        IServiceCollection services,
        IConfiguration configuration,
        Action<IRiderRegistrationConfigurator>? configureRider = null,
        Action<IBusRegistrationConfigurator>? configureBus = null,
        Action<IRiderRegistrationContext, IKafkaFactoryConfigurator>? configureKafkaEndpoints = null,
        bool useMessageScheduler = false)
        where TDbContext : DbContext
    {
        // Domain Bus
        services.AddTransient<IEventBus, Bus.MassTransitBus>();

        var kafkaHost = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        // MassTransit with Kafka and Outbox
        services.AddMassTransit(x =>
        {
            // Allow microservices to add consumers to the bus
            configureBus?.Invoke(x);

            // Register the message scheduler if enabled
            if (useMessageScheduler)
            {
                x.AddDelayedMessageScheduler();
            }

            // Configure Entity Framework Outbox for transactional consistency
            x.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                // Use PostgreSQL
                o.UsePostgres();
                
                // Query delay for polling outbox messages
                o.QueryDelay = TimeSpan.FromSeconds(1);
                
                // Enable the bus outbox so messages are published via the outbox
                o.UseBusOutbox();
            });

            // In-memory for internal messaging with optional scheduler
            x.UsingInMemory((context, cfg) =>
            {
                if (useMessageScheduler)
                {
                    // Enable in-memory message scheduler for delayed/scheduled messages
                    cfg.UseDelayedMessageScheduler();
                }
                
                cfg.ConfigureEndpoints(context);
            });

            // Kafka Rider for external events
            x.AddRider(rider =>
            {
                // Allow microservices to configure producers/consumers
                configureRider?.Invoke(rider);

                rider.UsingKafka((context, kafka) =>
                {
                    kafka.Host(kafkaHost);
                    
                    // Configure global Kafka settings
                    kafka.ClientId = "cryptojackpot";
                    
                    // Allow microservices to configure topic endpoints
                    configureKafkaEndpoints?.Invoke(context, kafka);
                });
            });
        });
    }

    /// <summary>
    /// Registers shared infrastructure with Kafka.
    /// </summary>
    /// <param name="services">The service collection used to register dependencies.</param>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="configureRider">Optional delegate to configure Kafka rider (for producers/consumers).</param>
    /// <param name="configureBus">Optional delegate to configure the message bus (for consumers).</param>
    /// <param name="configureKafkaEndpoints">Optional delegate to configure Kafka topic endpoints (for consumers).</param>
    /// <param name="useMessageScheduler">Determines whether to enable the in-memory message scheduler for delayed messages.</param>
    public static void RegisterServicesWithKafka(
        IServiceCollection services,
        IConfiguration configuration,
        Action<IRiderRegistrationConfigurator>? configureRider = null,
        Action<IBusRegistrationConfigurator>? configureBus = null,
        Action<IRiderRegistrationContext, IKafkaFactoryConfigurator>? configureKafkaEndpoints = null,
        bool useMessageScheduler = false)
    {
        // Domain Bus
        services.AddTransient<IEventBus, Bus.MassTransitBus>();

        var kafkaHost = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        // MassTransit with Kafka
        services.AddMassTransit(x =>
        {
            // Allow microservices to add consumers to the bus
            configureBus?.Invoke(x);

            // Register the message scheduler if enabled
            if (useMessageScheduler)
            {
                x.AddDelayedMessageScheduler();
            }

            // In-memory for internal messaging with optional scheduler
            x.UsingInMemory((context, cfg) =>
            {
                if (useMessageScheduler)
                {
                    // Enable in-memory message scheduler for delayed/scheduled messages
                    cfg.UseDelayedMessageScheduler();
                }
                
                cfg.ConfigureEndpoints(context);
            });

            // Kafka Rider for external events
            x.AddRider(rider =>
            {
                // Allow microservices to configure producers/consumers
                configureRider?.Invoke(rider);

                rider.UsingKafka((context, kafka) =>
                {
                    kafka.Host(kafkaHost);
                    
                    // Allow microservices to configure topic endpoints
                    configureKafkaEndpoints?.Invoke(context, kafka);
                });
            });
        });
    }
}