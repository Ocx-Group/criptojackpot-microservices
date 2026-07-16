using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// ==============================================
// OpenTelemetry
// ==============================================
var otelSection = builder.Configuration.GetSection("OpenTelemetry");
if (otelSection.GetValue("Enabled", true))
{
    var otelEndpoint = otelSection["Endpoint"] ?? "http://localhost:4317";
    var serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0";
    var environment = builder.Environment.EnvironmentName;

    void ConfigureOtlp(OtlpExporterOptions opts)
    {
        opts.Endpoint = new Uri(otelEndpoint);
        opts.Protocol = OtlpExportProtocol.Grpc;
    }

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r
            .AddService("cryptojackpot-bff", serviceVersion: serviceVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = environment
            }))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
            .AddHttpClientInstrumentation(opts => opts.RecordException = true)
            .AddOtlpExporter(ConfigureOtlp))
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(ConfigureOtlp));
}

// ==============================================
// YARP Reverse Proxy Configuration
// ==============================================
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ==============================================
// Authentication: JWT from Cookie HttpOnly
// ==============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var cookieSettings = builder.Configuration.GetSection("CookieSettings");
var accessTokenCookieName = cookieSettings["AccessTokenCookieName"] ?? "access_token";
var secretKey = jwtSettings["SecretKey"];

// Only configure JWT auth if settings are provided
if (!string.IsNullOrEmpty(secretKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey))
            };
            
            // Extract JWT from HttpOnly Cookie
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Try to get token from cookie first
                    if (context.Request.Cookies.TryGetValue(accessTokenCookieName, out var cookieToken))
                    {
                        context.Token = cookieToken;
                    }
                    // Fallback: Authorization header is handled automatically
                    return Task.CompletedTask;
                }
            };
        });
}
else
{
    // Development mode without JWT validation
    builder.Services.AddAuthentication();
}

builder.Services.AddAuthorization();

// ==============================================
// Health Checks for downstream services
// ==============================================
builder.Services.AddHealthChecks();

// ==============================================
// CORS Configuration
// ==============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3000" };
        
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ==============================================
// Logging
// ==============================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ==============================================
// Middleware Pipeline
// ==============================================

// CORS must be before routing
app.UseCors("AllowFrontend");

// Health check endpoint
app.MapHealthChecks("/health");

// WebSockets must be enabled before MapReverseProxy for SignalR support
app.UseWebSockets();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// YARP Reverse Proxy - routes all traffic to downstream services
app.MapReverseProxy();

// Fallback for root path
app.MapGet("/", () => Results.Ok(new 
{ 
    service = "CryptoJackpot BFF Gateway",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

await app.RunAsync();
