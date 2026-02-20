using CryptoJackpot.Identity.Data.Context;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace CryptoJackpot.Identity.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory con Testcontainers para integration tests.
/// </summary>
public class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // ─── Solo PostgreSQL ────────────────────
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("cryptojackpot_identity_test")
        .WithUsername("test")
        .WithPassword("test")
        .WithReuse(true) // Reutiliza container entre ejecuciones locales
        .Build();

    // ─── Lifecycle ───────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        // Solo PostgreSQL 
        await _postgres.StartAsync();

        // Migraciones y seed UNA sola vez para toda la suite
        EnsureDatabaseCreated();
        await DatabaseSeeder.SeedBaseDataAsync(Services);
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    // ─── WebApplicationFactory Override ──────────────────────────

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),

                // Dummy — nunca se conecta, TestHarness lo reemplaza
                ["Kafka:BootstrapServers"] = "localhost:19092",
                ["Kafka:DefaultPartitions"] = "1",
                ["Kafka:DefaultReplicationFactor"] = "1",

                ["JwtSettings:SecretKey"] = "integration_test_secret_key_minimum_32_chars_for_hmac_sha256!!",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:ExpirationInMinutes"] = "15",

                ["CookieSettings:Domain"] = null,
                ["CookieSettings:SecureOnly"] = "false",
                ["CookieSettings:SameSite"] = "Lax",
                ["CookieSettings:AccessTokenCookieName"] = "access_token",
                ["CookieSettings:RefreshTokenCookieName"] = "refresh_token",
                ["CookieSettings:Path"] = "/",

                ["DataProtection:ApplicationName"] = "CryptoJackpot.Identity.Tests",
                ["DataProtection:RedisConnectionString"] = null,

                ["GoogleAuth:ClientId"] = "test-client-id",
                ["GoogleAuth:ClientSecret"] = "test-client-secret",
                ["TwoFactor:Issuer"] = "CryptoJackpotTest",
                ["TwoFactor:ChallengeTokenMinutes"] = "5",
                ["TwoFactor:RecoveryCodeCount"] = "8",
                ["DigitalOcean:Endpoint"] = "https://test.digitaloceanspaces.com",
                ["DigitalOcean:BucketName"] = "test-bucket",
                ["DigitalOcean:AccessKey"] = "test",
                ["DigitalOcean:SecretKey"] = "test",
                ["DigitalOcean:Region"] = "nyc3",
                ["Cors:AllowedOrigins:0"] = "http://localhost:3000"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // ── 1. Reemplazar DbContext ──────────────────────────
            services.RemoveAll<DbContextOptions<IdentityDbContext>>();
            services.RemoveAll<IdentityDbContext>();

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_postgres.GetConnectionString());
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(dataSource)
                    .UseSnakeCaseNamingConvention());
            
            var massTransitDescriptors = services
                .Where(IsMassTransitDescriptor)
                .ToList();

            foreach (var descriptor in massTransitDescriptors)
                services.Remove(descriptor);

            // 2b: Limpiar health checks de MassTransit registrados en opciones
            services.Configure<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckServiceOptions>(options =>
            {
                var massTransitChecks = options.Registrations
                    .Where(r => r.Name.Contains("masstransit", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var check in massTransitChecks)
                    options.Registrations.Remove(check);
            });

            // 2c: TestHarness — bus in-memory que captura Publish() sin broker
            services.AddMassTransitTestHarness();
        });
    }

    private void EnsureDatabaseCreated()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        db.Database.Migrate();
    }

    /// <summary>
    /// HttpClient con CookieContainer para simular browser con BFF HttpOnly cookies.
    /// </summary>
    public HttpClient CreateClientWithCookies()
    {
        var handler = Server.CreateHandler();
        var cookieHandler = new CookieContainerHandler(handler);
        return new HttpClient(cookieHandler)
        {
            BaseAddress = new Uri("https://localhost")
        };
    }

    // ─── Helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Determina si un ServiceDescriptor pertenece a MassTransit, Confluent o Kafka.
    /// Cubre: IBus, IBusInstance, BusDepot, Riders, Outbox, HealthChecks internos, etc.
    /// </summary>
    private static bool IsMassTransitDescriptor(ServiceDescriptor d)
    {
        // Check service type
        if (IsMassTransitType(d.ServiceType))
            return true;

        // Check implementation type
        if (d.ImplementationType is not null && IsMassTransitType(d.ImplementationType))
            return true;

        // Check implementation factory (for lambda-registered services)
        // We can't inspect the lambda, but we can check the ServiceType namespace
        return false;
    }

    private static bool IsMassTransitType(Type type)
    {
        var ns = type.Namespace ?? string.Empty;
        var asm = type.Assembly.GetName().Name ?? string.Empty;

        return ns.StartsWith("MassTransit", StringComparison.Ordinal) ||
               ns.StartsWith("Confluent", StringComparison.Ordinal) ||
               asm.StartsWith("MassTransit", StringComparison.Ordinal) ||
               asm.StartsWith("Confluent", StringComparison.Ordinal);
    }
}

public class CookieContainerHandler : DelegatingHandler
{
    private readonly System.Net.CookieContainer _cookies = new();

    public CookieContainerHandler(HttpMessageHandler inner) : base(inner) { }

    public System.Net.CookieContainer Cookies => _cookies;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var cookieHeader = _cookies.GetCookieHeader(request.RequestUri!);
        if (!string.IsNullOrEmpty(cookieHeader))
            request.Headers.Add("Cookie", cookieHeader);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            foreach (var raw in setCookies)
            {
                try
                {
                    // Prefer framework parser (handles quoted values and full RFC semantics).
                    _cookies.SetCookies(request.RequestUri!, raw);
                }
                catch
                {
                    // Fallback parser for edge cases where SetCookies fails.
                    try
                    {
                        var cookie = ParseSetCookieHeader(raw);
                        if (cookie is not null)
                            _cookies.Add(request.RequestUri!, cookie);
                    }
                    catch
                    {
                        // Ignore malformed cookies
                    }
                }
            }
        }

        return response;
    }

    /// <summary>
    /// Manually parse a single Set-Cookie header into a <see cref="System.Net.Cookie"/>.
    /// 
    /// CookieContainer.SetCookies uses commas as cookie separators, which collides
    /// with the comma inside the "expires" date (e.g. "Thu, 20 Feb 2026 00:00:00 GMT").
    /// Parsing manually and using CookieContainer.Add avoids this pitfall.
    /// </summary>
    private static System.Net.Cookie? ParseSetCookieHeader(string header)
    {
        var parts = header.Split(';', StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return null;

        // First segment is name=value
        var nameValue = parts[0];
        var eqIdx = nameValue.IndexOf('=');
        if (eqIdx <= 0) return null;

        var name = nameValue[..eqIdx];
        var value = nameValue[(eqIdx + 1)..];

        var cookie = new System.Net.Cookie(name, value);

        for (var i = 1; i < parts.Length; i++)
        {
            var attr = parts[i];
            var lower = attr.ToLowerInvariant();

            if (lower.StartsWith("path="))
                cookie.Path = attr[(attr.IndexOf('=') + 1)..];
            else if (lower.StartsWith("domain="))
                cookie.Domain = attr[(attr.IndexOf('=') + 1)..];
            else if (lower.Equals("secure"))
                cookie.Secure = true;
            else if (lower.Equals("httponly"))
                cookie.HttpOnly = true;
            // expires and samesite are intentionally ignored:
            // - expires: CookieContainer manages expiry via MaxAge or defaults
            // - samesite: not supported by System.Net.Cookie
        }

        // Do not force an empty domain.
        // CookieContainer.Add(requestUri, cookie) will bind host-only cookies
        // when Domain is not explicitly set in Set-Cookie.

        return cookie;
    }
}
