namespace CryptoJackpot.Lottery.Application.DTOs;

/// <summary>
/// Detail of all series for a specific number.
/// </summary>
public class NumberSeriesDetailDto
{
    public Guid LotteryId { get; set; }
    public int Number { get; set; }
    public int TotalSeries { get; set; }
    public int SoldCount { get; set; }
    public int ReservedCount { get; set; }
    public int AvailableCount { get; set; }
    public List<SeriesStatusItemDto> Series { get; set; } = new();
}

/// <summary>
/// Status of a single series slot.
/// </summary>
public class SeriesStatusItemDto
{
    public int Series { get; set; }
    public string Status { get; set; } = "Available";
}
