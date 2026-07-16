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
    /// Zero-padded display representation persisted at generation time
    /// (e.g., "007" for Pick3, "0007" for a 0-9999 raffle).
    /// </summary>
    public string? DisplayNumber { get; set; }
}
