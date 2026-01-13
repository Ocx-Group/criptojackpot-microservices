using Microsoft.AspNetCore.Builder;

namespace CryptoJackpot.Domain.Core.Middleware;

/// <summary>
/// Extension methods for registering shared middleware in the application pipeline.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds the global exception handling middleware to the application pipeline.
    /// Should be registered early in the pipeline to catch all exceptions.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}

