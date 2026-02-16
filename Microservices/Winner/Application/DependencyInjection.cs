using System.Text;
using CryptoJackpot.Winner.Data.Context;
using CryptoJackpot.Infra.IoC;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

namespace CryptoJackpot.Winner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddWinnerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddAuthentication(services, configuration);
        AddDatabase(services, configuration);
        AddSwagger(services);
        AddControllers(services, configuration);
        AddRepositories(services);
        AddApplicationServices(services);
        AddInfrastructure(services, configuration);

        return services;
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
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        // Use AddDbContextPool to reuse DbContext instances (memory optimization)
        services.AddDbContextPool<WinnerDbContext>(options =>
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
                Title = "CryptoJackpot Winner API",
                Version = "v1",
                Description = "Winner microservice for winner management"
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
        // Add repositories here
    }

    private static void AddApplicationServices(IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
    }

    private static void AddInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        // Use shared infrastructure with Kafka and Transactional Outbox
        DependencyContainer.RegisterServicesWithKafka<WinnerDbContext>(
            services,
            configuration,
            configureRider: _ =>
            {
                // Register producers/consumers for events here
            },
            configureBus: null,
            configureKafkaEndpoints: null,
            useMessageScheduler: false);
    }
}