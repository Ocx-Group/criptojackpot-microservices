namespace CryptoJackpot.Lottery.Application.DTOs;

public class LotteryNumberDto
{
    public Guid Id { get; set; }
    public Guid LotteryId { get; set; }
    public int Number { get; set; }
    public int Series { get; set; }
    public bool IsAvailable { get; set; }
    public Guid? TicketId { get; set; }

    /// <summary>
    /// Formatted number string based on lottery type (e.g., "007" for Pick3).
    /// </summary>
    public string? FormattedNumber { get; set; }
}
