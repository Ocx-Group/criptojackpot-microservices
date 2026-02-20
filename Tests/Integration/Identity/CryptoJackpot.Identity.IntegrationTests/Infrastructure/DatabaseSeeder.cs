using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
namespace CryptoJackpot.Identity.IntegrationTests.Infrastructure;

public static class DatabaseSeeder
{
    /// <summary>
    /// Seed mínimo: crea el rol "User" y el país por defecto.
    /// Necesario porque el Identity service depende de estos registros base.
    /// </summary>
    public static async Task SeedBaseDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        // Seed Role if not exists
        if (!db.Roles.Any(r => r.Name == "User"))
        {
            db.Roles.Add(new Role
            {
                Name = "User",
                Description = "Default user role",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Seed Country if not exists
        if (!db.Countries.Any())
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
    /// Crea un usuario con password hasheado via BCrypt.
    /// Retorna el password en texto plano para usarlo en los tests de login.
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

        var role = db.Roles.First(r => r.Name == "User");
        var country = db.Countries.First();

        // BCrypt hash — mismo algoritmo que usa BcryptPasswordHasher en producción
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

    /// <summary>
    /// Crea un usuario exclusivamente Google (sin password local).
    /// </summary>
    public static async Task<User> CreateGoogleOnlyUserAsync(
        IServiceProvider services,
        string email = "google@cryptojackpot.com")
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var role = db.Roles.First(r => r.Name == "User");
        var country = db.Countries.First();

        var user = new User
        {
            UserGuid = Guid.NewGuid(),
            Name = "Google",
            LastName = "User",
            Email = email,
            EmailVerified = true,
            PasswordHash = null,
            GoogleId = "google_subject_id_12345",
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
    /// Limpia todos los datos de usuario (para usar con Respawn o entre tests).
    /// </summary>
    public static async Task CleanUsersAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        db.UserRefreshTokens.RemoveRange(db.UserRefreshTokens);
        db.UserRecoveryCodes.RemoveRange(db.UserRecoveryCodes);
        db.UserReferrals.RemoveRange(db.UserReferrals);
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();
    }
}