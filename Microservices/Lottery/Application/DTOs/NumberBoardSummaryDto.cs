namespace CryptoJackpot.Lottery.Application.DTOs;

/// <summary>
/// Summary of all numbers for the admin board view.
/// </summary>
public class NumberBoardSummaryDto
{
    public Guid LotteryId { get; set; }
    public int MinNumber { get; set; }
    public int MaxNumber { get; set; }
    public int TotalSeries { get; set; }
    public int TotalSlots { get; set; }
    public int SoldCount { get; set; }
    public int ReservedCount { get; set; }
    public int AvailableCount { get; set; }
    public List<NumberSummaryItemDto> Numbers { get; set; } = new();
}

/// <summary>
/// Per-number summary (sold/reserved/available counts).
/// </summary>
public class NumberSummaryItemDto
{
    public int Number { get; set; }
    public int SoldCount { get; set; }
    public int ReservedCount { get; set; }
    public int AvailableCount { get; set; }
}
