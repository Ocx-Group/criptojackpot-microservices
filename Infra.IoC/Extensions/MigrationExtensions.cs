using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CryptoJackpot.Infra.IoC.Extensions;

/// <summary>
/// Centralized migration extensions to ensure consistent migration handling across all microservices.
/// Handles race conditions when multiple replicas attempt to apply migrations concurrently.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Applies pending database migrations for the specified DbContext.
    /// Only runs in Development environment. Implements retry logic with exponential backoff
    /// to handle race conditions between multiple Kubernetes replicas.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type to migrate.</typeparam>
    /// <param name="host">The application host.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 5).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ApplyMigrationsAsync<TContext>(
        this IHost host,
        int maxRetries = 5) where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var env = services.GetRequiredService<IHostEnvironment>();

        // Only apply migrations in Development environment
        if (!env.IsDevelopment())
        {
            logger.LogInformation("[{Context}] Skipping migrations - not in Development environment.", typeof(TContext).Name);
            return;
        }

        var context = services.GetRequiredService<TContext>();
        var contextName = typeof(TContext).Name;

        // Retry with exponential backoff for race conditions between replicas
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                // Wait for database to be ready
                if (!await context.Database.CanConnectAsync())
                {
                    logger.LogInformation("[{Context}] Database not ready, waiting... (attempt {Attempt}/{Max})",
                        contextName, attempt, maxRetries);
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                    continue;
                }

                // Check for pending migrations
                var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();
                if (pending.Count == 0)
                {
                    logger.LogInformation("[{Context}] No pending migrations.", contextName);
                    return;
                }

                logger.LogInformation("[{Context}] Applying {Count} pending migrations (attempt {Attempt}/{Max}): {Migrations}",
                    contextName, pending.Count, attempt, maxRetries, string.Join(", ", pending));

                await context.Database.MigrateAsync();
                logger.LogInformation("[{Context}] Migrations applied successfully.", contextName);
                return;
            }
            catch (PostgresException ex) when (ex.SqlState == "42P07") // relation already exists
            {
                // Another replica already created the tables - this is expected in concurrent scenarios
                logger.LogWarning("[{Context}] Tables already exist (concurrent migration detected). Skipping. SqlState: {SqlState}",
                    contextName, ex.SqlState);
                return;
            }
            catch (PostgresException ex) when (ex.SqlState == "55P03") // lock_not_available
            {
                // Another replica is currently holding the migration lock
                logger.LogWarning("[{Context}] Migration lock held by another instance. Retry {Attempt}/{Max}... SqlState: {SqlState}",
                    contextName, attempt, maxRetries, ex.SqlState);
                
                if (attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 3));
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // unique_violation (duplicate key)
            {
                // Migration history entry already exists - another replica completed the migration
                logger.LogWarning("[{Context}] Migration already recorded by another instance. SqlState: {SqlState}",
                    contextName, ex.SqlState);
                return;
            }
            catch (PostgresException ex) when (ex.SqlState == "40001") // serialization_failure
            {
                // Transaction conflict - another replica is migrating
                logger.LogWarning("[{Context}] Transaction conflict during migration. Retry {Attempt}/{Max}... SqlState: {SqlState}",
                    contextName, attempt, maxRetries, ex.SqlState);
                
                if (attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                }
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex, "[{Context}] Migration failed (attempt {Attempt}/{Max}). Retrying...",
                    contextName, attempt, maxRetries);
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
            }
        }

        // Log error but don't throw - allow app to start even if migrations fail in development
        logger.LogError("[{Context}] Failed to apply migrations after {Max} attempts. Application will continue but may encounter database schema issues.",
            contextName, maxRetries);
    }
}

