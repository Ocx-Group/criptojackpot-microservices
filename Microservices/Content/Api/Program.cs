using CryptoJackpot.Content.Data.Context;
using CryptoJackpot.Content.Infra.IoC;
using CryptoJackpot.Domain.Core.Middleware;
using CryptoJackpot.Infra.IoC.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Single point of DI configuration
builder.Services.AddContentServices(builder.Configuration);

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

// Apply migrations in development
await app.ApplyMigrationsAsync<ContentDbContext>();

await app.RunAsync();
