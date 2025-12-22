namespace CryptoJackpot.Identity.Domain.Models;

public class User : BaseEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Identification { get; set; }
    public string? Phone { get; set; }
    public long CountryId { get; set; }
    public string StatePlace { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? Address { get; set; }
    public bool Status { get; set; }
    public string? ImagePath { get; set; }
    public string? GoogleAccessToken { get; set; }
    public string? GoogleRefreshToken { get; set; }
    public string? SecurityCode { get; set; }
    public DateTime? PasswordResetCodeExpiration { get; set; }
    public long RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Country Country { get; set; } = null!;

    // Navegación: Usuarios que este usuario ha referido
    public ICollection<UserReferral> Referrals { get; set; } = new List<UserReferral>();

    // Navegación: Referido por 
    public UserReferral? ReferredBy { get; set; }
}