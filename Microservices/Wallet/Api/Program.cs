using CryptoJackpot.Infra.IoC.Extensions;
using CryptoJackpot.Wallet.Api.Services;
using CryptoJackpot.Wallet.Data.Context;
using CryptoJackpot.Wallet.Infra.IoC;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// gRPC requires HTTP/2. Dedicated port for HTTP/2-only traffic.
var httpPort = builder.Configuration.GetValue("Kestrel:HttpPort", 8080);
var grpcPort = builder.Configuration.GetValue("Kestrel:GrpcPort", 5052);
builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.ListenAnyIP(httpPort);
    kestrel.ListenAnyIP(grpcPort, o => o.Protocols = HttpProtocols.Http2);
});

// Single point of DI configuration
builder.Services.AddWalletServices(builder.Configuration);

// gRPC server — allows other microservices to debit wallet balance
builder.Services.AddGrpc();

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

// gRPC endpoints — internal cluster communication only
app.MapGrpcService<WalletDebitGrpcServiceImpl>();

// Apply migrations in development
await app.ApplyMigrationsAsync<WalletDbContext>();

await app.RunAsync();

