using System.ComponentModel.DataAnnotations;

namespace CryptoJackpot.Identity.Application.DTOs;

public class RequestPasswordResetRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}

