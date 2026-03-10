using CryptoJackpot.Domain.Core.Middleware;
using CryptoJackpot.Identity.Api.Services;
using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Infra.IoC;
using CryptoJackpot.Infra.IoC.Extensions;

var builder = WebApplication.CreateBuilder(args);

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
