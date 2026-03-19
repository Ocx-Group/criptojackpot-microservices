using CryptoJackpot.Content.Domain.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Content.Data.Context;

public class ContentDbContext : DbContext
{
    public DbSet<Testimonial> Testimonials { get; set; }

    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);

        // MassTransit Outbox configuration with snake_case naming
        modelBuilder.AddInboxStateEntity(x => x.ToTable("inbox_state"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("outbox_message"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("outbox_state"));
    }
}
