namespace CryptoJackpot.Identity.Application.Requests;

public class ResetPasswordWithCodeRequest
{
    public string Email { get; set; } = null!;
    public string SecurityCode { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}
