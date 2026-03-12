namespace CryptoJackpot.Order.Application.DTOs;

/// <summary>
/// DTO for admin dashboard order/ticket statistics.
/// </summary>
public class OrderStatsDto
{
    public int TotalTicketsSold { get; set; }
    public int TicketsThisMonth { get; set; }
    public int TicketsLastMonth { get; set; }
    public decimal TicketsPercentageChange { get; set; }

    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public decimal RevenuePercentageChange { get; set; }

    public decimal ConversionRate { get; set; }
    public decimal ConversionRateLastMonth { get; set; }
    public decimal ConversionRateChange { get; set; }
}
