namespace CryptoJackpot.Identity.Application.Configuration;

public class JwtConfig
{
    public string SecretKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpirationInMinutes { get; set; }
}

public class ApplicationConfiguration
{
    public JwtConfig? JwtSettings { get; init; }
    // Add others if needed later
}
