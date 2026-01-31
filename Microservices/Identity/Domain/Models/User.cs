using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Identity.Domain.Models;

public class User : BaseEntity
{
    /// <summary>
    /// External GUID for API exposure and cross-service communication
    /// </summary>
    public Guid UserGuid { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Keycloak user ID for authentication integration.
    /// All auth operations (login, password reset, email verification) are handled by Keycloak.
    /// </summary>
    public string? KeycloakId { get; set; }
    
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Identification { get; set; }
    public string? Phone { get; set; }
    public long CountryId { get; set; }
    public string StatePlace { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? Address { get; set; }
    
    /// <summary>
    /// User status synchronized with Keycloak's email verified status
    /// </summary>
    public bool Status { get; set; }
    public string? ImagePath { get; set; }
    public long RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Country Country { get; set; } = null!;

    // Navegación: Usuarios que este usuario ha referido
    public ICollection<UserReferral> Referrals { get; set; } = new List<UserReferral>();

    // Navegación: Referido por 
    public UserReferral? ReferredBy { get; set; }
}
