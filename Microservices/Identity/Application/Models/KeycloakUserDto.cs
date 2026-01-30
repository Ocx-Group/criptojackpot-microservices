namespace CryptoJackpot.Identity.Application.Models;

/// <summary>
/// Represents a user from Keycloak Admin API.
/// </summary>
public class KeycloakUserDto
{
    /// <summary>
    /// The unique Keycloak user ID (UUID).
    /// </summary>
    public string Id { get; set; } = null!;
    
    /// <summary>
    /// The username (typically same as email).
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// The user's email address.
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// The user's first name.
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// The user's last name.
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Whether the user's email has been verified.
    /// </summary>
    public bool EmailVerified { get; set; }
    
    /// <summary>
    /// Whether the user account is enabled.
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Custom user attributes (e.g., user_id, phone).
    /// </summary>
    public Dictionary<string, List<string>>? Attributes { get; set; }
    
    /// <summary>
    /// The timestamp when the user was created.
    /// </summary>
    public long? CreatedTimestamp { get; set; }
    
    /// <summary>
    /// Required actions the user must complete (e.g., UPDATE_PASSWORD).
    /// </summary>
    public List<string>? RequiredActions { get; set; }
}
