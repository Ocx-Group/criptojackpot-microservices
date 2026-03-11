using System.Text;
using Asp.Versioning;
using CryptoJackpot.Domain.Core.Behaviors;
using CryptoJackpot.Domain.Core.Constants;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
using CryptoJackpot.Domain.Core.Protos;
using CryptoJackpot.Infra.IoC;
using CryptoJackpot.Infra.IoC.Extensions;
using CryptoJackpot.Wallet.Application;
using CryptoJackpot.Wallet.Application.Consumers;
using CryptoJackpot.Wallet.Application.Services;
using CryptoJackpot.Wallet.Data.Context;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

namespace CryptoJackpot.Wallet.Infra.IoC;

public static class IoCExtension
{
    public static void AddWalletServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        DependencyContainer.RegisterOpenTelemetry(services, configuration, "cryptojackpot-wallet");
        AddAuthentication(services, configuration);
        AddDatabase(services, configuration);
        AddSwagger(services);
        AddControllers(services, configuration);
        AddRepositories(services);
        AddApplicationServices(services);
        AddGrpcClients(services, configuration);
        AddRedisCache(services, configuration);
        AddInfrastructure(services, configuration);
    }

    private static void AddRedisCache(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "wallet:";
            });
        }
        else
        {
            // Fallback to in-memory cache when Redis is not configured (local dev)
            services.AddDistributedMemoryCache();
        }
    }


    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        // JWT authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured");

        // Cookie settings for extracting token from HTTP-only cookies
        var cookieSettings = configuration.GetSection("CookieSettings");
        var accessTokenCookieName = cookieSettings["AccessTokenCookieName"] ?? "access_token";

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    };

                // Only accept JWT from HTTP-only cookie (no Authorization header)
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(accessTokenCookieName, out var cookieToken) 
                            && !string.IsNullOrEmpty(cookieToken))
                        {
                            context.Token = cookieToken;
                        }
                        else
                        {
                            context.NoResult();
                        }
                        return Task.CompletedTask;
                    }
                };
                });
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured");

        // Configure Npgsql DataSource
        // When using PgBouncer in transaction mode, Npgsql's internal pooling works alongside it
        // PgBouncer handles the real connection pool to PostgreSQL (DEFAULT_POOL_SIZE=20)
        // Npgsql manages virtual connections from the application side
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        // Use AddDbContextPool to reuse DbContext instances (memory optimization)
        // This reduces object creation overhead in high-concurrency scenarios
        // The poolSize here is for DbContext instances, not database connections
        services.AddDbContextPool<WalletDbContext>(options =>
            options.UseNpgsql(dataSource)
                .UseSnakeCaseNamingConvention(),
            poolSize: 100);
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CryptoJackpot Wallet API",
                Version = "v1",
                Description = "Wallet microservice for wallet management"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token in format: {token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    private static void AddControllers(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                if (allowedOrigins.Length > 0)
                {
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
                else
                {
                    // For development without specific origins - need SetIsOriginAllowed for credentials
                    // Note: AllowAnyOrigin() cannot be used with AllowCredentials()
                    builder.SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
            });
        });
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUserCryptoWalletRepository, Data.Repositories.UserCryptoWalletRepository>();
        services.AddScoped<IWalletRepository, Data.Repositories.WalletTransactionRepository>();
        services.AddScoped<IWalletBalanceRepository, Data.Repositories.WalletBalanceRepository>();
        services.AddScoped<IWithdrawalRequestRepository, Data.Repositories.WithdrawalRequestRepository>();
        services.AddScoped<IUnitOfWork, Data.UnitOfWork>();
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        var assembly = typeof(IAssemblyReference).Assembly;

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        // Validators
        services.AddValidatorsFromAssembly(assembly);
        
        // Behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        // AutoMapper
        services.AddAutoMapper(assembly);
        services.AddScoped<IWalletService, WalletService>();
    }

    private static void AddGrpcClients(IServiceCollection services, IConfiguration configuration)
    {
        var identityGrpcAddress = configuration["GrpcServices:IdentityAddress"]
                                  ?? "http://identity-api:80";

        services.AddGrpcClient<ReferralGrpcService.ReferralGrpcServiceClient>(options =>
            {
                options.Address = new Uri(identityGrpcAddress);
            })
            .ConfigureChannel(channel =>
            {
                // Deadline for all calls — prevents hanging if Identity is unresponsive
                channel.HttpHandler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30)
                };
            });

        services.AddGrpcClient<UserVerificationGrpcService.UserVerificationGrpcServiceClient>(options =>
            {
                options.Address = new Uri(identityGrpcAddress);
            })
            .ConfigureChannel(channel =>
            {
                channel.HttpHandler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30)
                };
            });

        services.AddScoped<IReferralGrpcClient, ReferralGrpcClient>();
        services.AddScoped<IUserVerificationGrpcClient, UserVerificationGrpcClient>();
    }

    private static void AddInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        // Use shared infrastructure with Kafka and Transactional Outbox
        DependencyContainer.RegisterServicesWithKafka<WalletDbContext>(
            services,
            configuration,
            configureRider: rider =>
            {
                // Register consumers
                rider.AddConsumer<OrderCompletedConsumer>();

                // Register producers
                rider.AddProducer<WithdrawalVerificationRequestedEvent>(KafkaTopics.WithdrawalVerificationRequested);
                rider.AddProducer<ReferralCommissionCreditedEvent>(KafkaTopics.ReferralCommissionCredited);
            },
            configureBus: null,
            configureKafkaEndpoints: (context, kafka) =>
            {

                // Order events - credit 1% referral purchase commission to the referrer
                kafka.TopicEndpoint<OrderCompletedEvent>(
                    KafkaTopics.OrderCompleted,
                    KafkaTopics.WalletGroup,
                    e =>
                    {
                        e.ConfigureConsumer<OrderCompletedConsumer>(context);
                        e.ConfigureTopicDefaults(configuration);
                    });
            },
            useMessageScheduler: false);
    }
}