using CryptoJackpot.Infra.IoC.Extensions;
using CryptoJackpot.Winner.Application;
using CryptoJackpot.Winner.Data.Context;

var builder = WebApplication.CreateBuilder(args);

// Single point of DI configuration
builder.Services.AddWinnerServices(builder.Configuration);

// Health Checks for Kubernetes probes
builder.Services.AddHealthChecks();

var app = builder.Build();

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
await app.ApplyMigrationsAsync<WinnerDbContext>();

await app.RunAsync();
