using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Identity.Data.Context;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    // Add other DbSets as needed (Permission, Country, etc.)

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Apply configurations here or load from assembly
    }
}
