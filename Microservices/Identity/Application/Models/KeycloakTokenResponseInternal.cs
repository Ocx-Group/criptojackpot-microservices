using System.Text.Json.Serialization;

namespace CryptoJackpot.Identity.Application.Models;

/// <summary>
/// Internal model for deserializing Keycloak token responses.
/// </summary>
public sealed class KeycloakTokenResponseInternal
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;
    
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    
    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }
}
