using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoJackpot.Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Utilidades para poblar y limpiar la base de datos de test.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seed mínimo: Role "User" + Country por defecto.
    /// Idempotente — se llama UNA vez desde IdentityApiFactory.InitializeAsync().
    /// </summary>
    public static async Task SeedBaseDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        if (!await db.Roles.AnyAsync(r => r.Name == "User"))
        {
            db.Roles.Add(new Role
            {
                Name = "User",
                Description = "Default user role",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (!await db.Countries.AnyAsync())
        {
            db.Countries.Add(new Country
            {
                Name = "Costa Rica",
                Iso2 = "CR",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Crea un usuario con BCrypt hash real.
    /// Retorna el password en texto plano para los tests.
    /// </summary>
    public static async Task<(User User, string PlainPassword)> CreateVerifiedUserAsync(
        IServiceProvider services,
        string email = "integration@cryptojackpot.com",
        string password = "TestPassword123!",
        bool emailVerified = true,
        bool twoFactorEnabled = false)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var role = await db.Roles.FirstAsync(r => r.Name == "User");
        var country = await db.Countries.FirstAsync();

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            UserGuid = Guid.NewGuid(),
            Name = "Integration",
            LastName = "Tester",
            Email = email,
            EmailVerified = emailVerified,
            PasswordHash = passwordHash,
            RoleId = role.Id,
            CountryId = country.Id,
            StatePlace = "San José",
            City = "San José",
            Status = true,
            TwoFactorEnabled = twoFactorEnabled,
            FailedLoginAttempts = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (user, password);
    }

    public static async Task<User> CreateGoogleOnlyUserAsync(
        IServiceProvider services,
        string email = "google@cryptojackpot.com")
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var role = await db.Roles.FirstAsync(r => r.Name == "User");
        var country = await db.Countries.FirstAsync();

        var user = new User
        {
            UserGuid = Guid.NewGuid(),
            Name = "Google",
            LastName = "User",
            Email = email,
            EmailVerified = true,
            PasswordHash = null,
            GoogleId = $"google_{Guid.NewGuid():N}", // Único por test
            RoleId = role.Id,
            CountryId = country.Id,
            StatePlace = "Test",
            City = "Test",
            Status = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Limpia datos de usuario entre tests.
    /// </summary>
    public static async Task CleanUsersAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        // Orden: FK dependencias primero → tabla principal última
        // ExecuteDeleteAsync genera un solo DELETE FROM por tabla (sin SELECT previo)
        await db.UserRefreshTokens.ExecuteDeleteAsync();
        await db.UserRecoveryCodes.ExecuteDeleteAsync();
        await db.UserReferrals.ExecuteDeleteAsync();
        await db.Users.ExecuteDeleteAsync();
    }
}