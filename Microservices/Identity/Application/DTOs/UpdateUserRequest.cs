using System.ComponentModel.DataAnnotations;

namespace CryptoJackpot.Identity.Application.DTOs;

public class UpdateUserRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Password { get; set; }
}
