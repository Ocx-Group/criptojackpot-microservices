namespace CryptoJackpot.Identity.Application.DTOs;

public class UserDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? ImagePath { get; set; }
    public string Token { get; set; } = null!;
    public RoleDto Role { get; set; } = null!;
}

public class RoleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
}
