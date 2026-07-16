using CryptoJackpot.Lottery.Domain.Enums;

namespace CryptoJackpot.Lottery.Application.DTOs;

/// <summary>
/// DTO for number status updates (for broadcast).
/// </summary>
public class NumberStatusDto
{
    public long NumberId { get; set; }
    public Guid LotteryNumberGuid { get; set; }
    public int Number { get; set; }
    public string DisplayNumber { get; set; } = null!;
    public int Series { get; set; }
    public NumberStatus Status { get; set; }
}

