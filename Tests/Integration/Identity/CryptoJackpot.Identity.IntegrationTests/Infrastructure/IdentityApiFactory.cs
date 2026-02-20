using CryptoJackpot.Identity.Data.Context;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.Redpanda;
using Xunit;

namespace CryptoJackpot.Identity.IntegrationTests.Infrastructure;

public class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // ─── Testcontainers ──────────────────────────────────────────

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("cryptojackpot_identity_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly RedpandaContainer _redpanda = new RedpandaBuilder()
        .WithImage("docker.redpanda.com/redpandadata/redpanda:v23.3.5")
        .Build();

    // ─── Public accessors para los tests ─────────────────────────

    public string PostgresConnectionString => _postgres.GetConnectionString();
    public string RedpandaBootstrapServers => _redpanda.GetBootstrapAddress();

    // ─── Lifecycle ───────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        // Levantar contenedores en paralelo para reducir tiempo de startup
        await Task.WhenAll(
            _postgres.StartAsync(),
            _redpanda.StartAsync());

        // Aplicar migraciones después de que los containers estén listos
        EnsureDatabaseCreated();

        // Iniciar MassTransit TestHarness para capturar eventos publicados
        var harness = Services.GetRequiredService<ITestHarness>();
        await harness.Start();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redpanda.DisposeAsync();
        await base.DisposeAsync();
    }

    // ─── WebApplicationFactory Override ──────────────────────────

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Override appsettings con valores de los containers efímeros
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // PostgreSQL → apunta al container
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),

                // Kafka/Redpanda → apunta al container
                ["Kafka:BootstrapServers"] = _redpanda.GetBootstrapAddress(),
                ["Kafka:DefaultPartitions"] = "1",
                ["Kafka:DefaultReplicationFactor"] = "1",

                // JWT determinístico para tests
                ["JwtSettings:SecretKey"] = "integration_test_secret_key_minimum_32_chars_for_hmac_sha256!!",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:ExpirationInMinutes"] = "15",

                // Cookie settings
                ["CookieSettings:Domain"] = null,
                ["CookieSettings:SecureOnly"] = "false",
                ["CookieSettings:SameSite"] = "Lax",
                ["CookieSettings:AccessTokenCookieName"] = "access_token",
                ["CookieSettings:RefreshTokenCookieName"] = "refresh_token",
                ["CookieSettings:Path"] = "/",

                // DataProtection sin Redis para tests
                ["DataProtection:ApplicationName"] = "CryptoJackpot.Identity.Tests",
                ["DataProtection:RedisConnectionString"] = null,

                // Google Auth (valores dummy — no se usa en login con password)
                ["GoogleAuth:ClientId"] = "test-client-id",
                ["GoogleAuth:ClientSecret"] = "test-client-secret",

                // 2FA
                ["TwoFactor:Issuer"] = "CryptoJackpotTest",
                ["TwoFactor:ChallengeTokenMinutes"] = "5",
                ["TwoFactor:RecoveryCodeCount"] = "8",

                // DigitalOcean Spaces (dummy)
                ["DigitalOcean:Endpoint"] = "https://test.digitaloceanspaces.com",
                ["DigitalOcean:BucketName"] = "test-bucket",
                ["DigitalOcean:AccessKey"] = "test",
                ["DigitalOcean:SecretKey"] = "test",
                ["DigitalOcean:Region"] = "nyc3",

                // CORS
                ["Cors:AllowedOrigins:0"] = "http://localhost:3000"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // ── Reemplazar DbContext para usar Testcontainers PostgreSQL ──
            services.RemoveAll<DbContextOptions<IdentityDbContext>>();
            services.RemoveAll<IdentityDbContext>();

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_postgres.GetConnectionString());
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(dataSource)
                    .UseSnakeCaseNamingConvention());

            // ── MassTransit: REMOVER configuración de Kafka completamente ──
            // El problema es que el IoC de producción registra MassTransit con Kafka (Rider)
            // y AddMassTransitTestHarness no lo sobreescribe correctamente.
            // Solución: remover descriptores de MassTransit y re-registrar con TestHarness
            
            var massTransitDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("MassTransit") == true ||
                            d.ImplementationType?.FullName?.Contains("MassTransit") == true ||
                            d.ImplementationType?.FullName?.Contains("Kafka") == true ||
                            d.ImplementationType?.FullName?.Contains("Rider") == true)
                .ToList();
            
            foreach (var descriptor in massTransitDescriptors)
            {
                services.Remove(descriptor);
            }
            
            // También remover el IBus y IBusControl registrados
            services.RemoveAll<IBus>();
            services.RemoveAll<IBusControl>();
            
            // Re-agregar MassTransit con TestHarness (in-memory, sin Kafka)
            services.AddMassTransitTestHarness(cfg =>
            {
                // El harness captura todos los mensajes publicados sin necesidad de Kafka
            });
        });
    }

    /// <summary>
    /// Aplica migraciones de base de datos. Llamar después de crear el factory.
    /// </summary>
    public void EnsureDatabaseCreated()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        db.Database.Migrate();
    }

    /// <summary>
    /// Crea un HttpClient que preserva cookies entre requests,
    /// emulando el comportamiento real del BFF con HttpOnly cookies.
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
}

/// <summary>
/// DelegatingHandler que mantiene un CookieContainer para simular
/// el flujo real de HttpOnly cookies en el navegador.
/// Sin esto, las cookies de access_token y refresh_token se pierden entre requests.
/// </summary>
public class CookieContainerHandler : DelegatingHandler
{
    private readonly System.Net.CookieContainer _cookies = new();

    public CookieContainerHandler(HttpMessageHandler inner) : base(inner) { }

    public System.Net.CookieContainer Cookies => _cookies;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Adjuntar cookies existentes al request
        var cookieHeader = _cookies.GetCookieHeader(request.RequestUri!);
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Capturar cookies del response (HttpOnly cookies del BFF)
        if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            foreach (var cookie in setCookies)
            {
                _cookies.SetCookies(request.RequestUri!, cookie);
            }
        }

        return response;
    }
}