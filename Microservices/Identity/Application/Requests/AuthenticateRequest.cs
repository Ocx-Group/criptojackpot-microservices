namespace CryptoJackpot.Identity.Application.Requests;

public class AuthenticateRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
