using CryptoJackpot.Domain.Core.Middleware;
using CryptoJackpot.Identity.Api.Services;
using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Infra.IoC;
using CryptoJackpot.Infra.IoC.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// gRPC requires HTTP/2. Without TLS, Http1AndHttp2 falls back to HTTP/1.1 only
// (Kestrel needs ALPN/TLS for protocol negotiation). Solution: dedicated HTTP/2 port.
// Note: ConfigureKestrel overrides ASPNETCORE_URLS, so we must bind both ports explicitly.
var httpPort = builder.Configuration.GetValue("Kestrel:HttpPort", 8080);
var grpcPort = builder.Configuration.GetValue("Kestrel:GrpcPort", 5001);
builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.ListenAnyIP(httpPort);
    kestrel.ListenAnyIP(grpcPort, o => o.Protocols = HttpProtocols.Http2);
});

// Single point of DI configuration
builder.Services.AddIdentityServices(builder.Configuration);

// gRPC server — allows other microservices to query Identity as source of truth
builder.Services.AddGrpc();

// Health Checks for Kubernetes probes
builder.Services.AddHealthChecks();

var app = builder.Build();

// Global exception handling - must be first in pipeline
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint for Kubernetes liveness/readiness probes
app.MapHealthChecks("/health");
app.MapControllers();

// gRPC endpoints — internal cluster communication only
app.MapGrpcService<ReferralGrpcServiceImpl>();

// Apply migrations in development
await app.ApplyMigrationsAsync<IdentityDbContext>();

await app.RunAsync();

// Necesario para WebApplicationFactory en tests de integración
public partial class Program
{
    protected Program() { }
}
