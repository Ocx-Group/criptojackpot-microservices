using CryptoJackpot.Identity.Application.Handlers;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoJackpot.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AuthenticateCommandHandler).Assembly));

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        return services;
    }
}