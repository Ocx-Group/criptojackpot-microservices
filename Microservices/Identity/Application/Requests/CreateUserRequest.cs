using System.ComponentModel.DataAnnotations;

namespace CryptoJackpot.Identity.Application.Requests;

public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }

    public long? CountryId { get; set; }

    public string? ReferralCode { get; set; }
}

