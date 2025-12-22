namespace CryptoJackpot.Identity.Application.DTOs;

public class AuthenticateRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
