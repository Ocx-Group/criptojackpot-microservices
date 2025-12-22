using CryptoJackpot.Identity.Application.Handlers;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoJackpot.Identity.Application;

public static class IdentityApplicationExtensions
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        // MediatR - register all handlers from Application assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AuthenticateCommandHandler).Assembly));

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        return services;
    }
}

