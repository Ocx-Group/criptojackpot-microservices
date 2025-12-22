using CryptoJackpot.Identity.Domain.Enums;
namespace CryptoJackpot.Identity.Domain.Models;

public class RolePermission : BaseEntity
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public PermissionLevel AccessLevel { get; set; }
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}