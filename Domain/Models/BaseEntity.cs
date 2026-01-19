namespace CryptoJackpot.Domain.Core.Models;

/// <summary>
/// Base entity with common audit fields.
/// All domain entities should inherit from this class.
/// </summary>
public abstract class BaseEntity
{
    public long  Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}

