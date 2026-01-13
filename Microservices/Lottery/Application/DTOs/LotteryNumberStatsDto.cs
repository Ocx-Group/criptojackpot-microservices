namespace CryptoJackpot.Lottery.Application.DTOs;

public class LotteryNumberStatsDto
{
    public Guid LotteryId { get; set; }
    public int TotalNumbers { get; set; }
    public int SoldNumbers { get; set; }
    public int AvailableNumbers { get; set; }
    public decimal PercentageSold { get; set; }
}

