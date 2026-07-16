namespace CryptoJackpot.Identity.Application.DTOs;

/// <summary>
/// DTO for admin dashboard user statistics.
/// </summary>
public class UserStatsDto
{
    public int TotalUsers { get; set; }
    public int UsersThisMonth { get; set; }
    public int UsersLastMonth { get; set; }
    public decimal PercentageChange { get; set; }
}
