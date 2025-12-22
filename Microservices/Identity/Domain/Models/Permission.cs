namespace CryptoJackpot.Identity.Domain.Models;

public class Permission : BaseEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Module { get; set; } = null!;

    public ICollection<RolePermission> RolePermissions { get; set; } = null!;
}