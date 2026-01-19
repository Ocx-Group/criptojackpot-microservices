using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Identity.Domain.Models;

public class Role : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = null!;
    public ICollection<RolePermission> RolePermissions { get; set; } = null!;
}