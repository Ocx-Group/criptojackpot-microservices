using CryptoJackpot.Lottery.Domain.Enums;

namespace CryptoJackpot.Lottery.Application.Utilities;

/// <summary>
/// Utility for formatting lottery numbers based on lottery type.
/// Pick3 numbers are always displayed as 3-digit strings (e.g., 7 → "007").
/// </summary>
public static class LotteryNumberFormatter
{
    public static string FormatNumber(int number, LotteryType type)
    {
        return type switch
        {
            LotteryType.Pick3 => number.ToString("D3"),
            _ => number.ToString()
        };
    }

    public static int GetNumberDigits(LotteryType type)
    {
        return type switch
        {
            LotteryType.Pick3 => 3,
            _ => 0
        };
    }
}
