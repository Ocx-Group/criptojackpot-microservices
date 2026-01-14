namespace CryptoJackpot.Lottery.Application.Utilities;

public static class LotteryNumberGenerator
{
    /// <summary>
    /// Generates a unique lottery number with format: LOT-{timestamp}-{random}
    /// </summary>
    /// <returns>A unique lottery number string</returns>
    public static string Generate()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Random.Shared.Next(1000, 9999);
        return $"LOT-{timestamp}-{random}";
    }
}

