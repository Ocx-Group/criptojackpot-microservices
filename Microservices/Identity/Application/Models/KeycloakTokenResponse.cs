namespace CryptoJackpot.Identity.Application.Models;

/// <summary>
/// Represents a token response from Keycloak token endpoint.
/// This is the public DTO returned by the service.
/// </summary>
public class KeycloakTokenResponse
{
    /// <summary>
    /// The access token (JWT) for authenticating API requests.
    /// </summary>
    public string AccessToken { get; set; } = null!;
    
    /// <summary>
    /// The refresh token for obtaining new access tokens.
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// The number of seconds until the access token expires.
    /// </summary>
    public int ExpiresIn { get; set; }
    
    /// <summary>
    /// The number of seconds until the refresh token expires.
    /// </summary>
    public int RefreshExpiresIn { get; set; }
    
    /// <summary>
    /// The token type (typically "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
    
    /// <summary>
    /// The ID token (JWT) containing user identity claims.
    /// Only present when openid scope is requested.
    /// </summary>
    public string? IdToken { get; set; }
    
    /// <summary>
    /// The scope granted by the authorization server.
    /// </summary>
    public string? Scope { get; set; }
    
    /// <summary>
    /// The session state for session management.
    /// </summary>
    public string? SessionState { get; set; }
}
