using System.ComponentModel.DataAnnotations;

namespace CryptoJackpot.Identity.Application.Requests;

public class RequestPasswordResetRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}

