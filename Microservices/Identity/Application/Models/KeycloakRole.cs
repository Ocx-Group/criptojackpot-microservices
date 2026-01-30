namespace CryptoJackpot.Identity.Application.Models;

/// <summary>
/// Represents a role from Keycloak Admin API.
/// </summary>
public sealed class KeycloakRole
{
    /// <summary>
    /// The unique identifier of the role in Keycloak.
    /// </summary>
    public string Id { get; set; } = null!;
    
    /// <summary>
    /// The name of the role.
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Optional description of the role.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether this is a composite role (contains other roles).
    /// </summary>
    public bool Composite { get; set; }
    
    /// <summary>
    /// Whether this is a client-level role.
    /// </summary>
    public bool ClientRole { get; set; }
    
    /// <summary>
    /// The container ID (realm or client ID).
    /// </summary>
    public string? ContainerId { get; set; }
}
