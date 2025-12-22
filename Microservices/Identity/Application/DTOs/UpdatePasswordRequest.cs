using System.ComponentModel.DataAnnotations;

namespace CryptoJackpot.Identity.Application.DTOs;

public class UpdatePasswordRequest
{
    [Required]
    public long UserId { get; set; }

    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}

