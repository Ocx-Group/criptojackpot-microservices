using CryptoJackpot.Lottery.Domain.Enums;

namespace CryptoJackpot.Lottery.Application.Utilities;

/// <summary>
/// Utility for formatting lottery numbers based on lottery type and range.
/// Pick3 numbers are always 3-digit strings (e.g., 7 → "007"); other types
/// derive their width from the lottery's max number (e.g., 9999 → "0007").
/// This is the single source of truth used to persist DisplayNumber at
/// number-generation time.
/// </summary>
public static class LotteryNumberFormatter
{
    public static string FormatNumber(int number, LotteryType type, int maxNumber)
    {
        return number.ToString().PadLeft(GetNumberDigits(type, maxNumber), '0');
    }

    public static int GetNumberDigits(LotteryType type, int maxNumber)
    {
        return type == LotteryType.Pick3
            ? 3
            : Math.Max(2, maxNumber.ToString().Length);
    }
}
