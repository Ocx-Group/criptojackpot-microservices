namespace CryptoJackpot.Lottery.Domain.Configuration;

/// <summary>
/// Configuration for how long a number reservation (and therefore the resulting order)
/// stays valid before it expires and the numbers are released back to the pool.
/// Bound from the "ReservationSettings" configuration section.
/// </summary>
public class ReservationSettings
{
    public const string SectionName = "ReservationSettings";

    /// <summary>
    /// Minutes a reservation stays valid, giving the user time to complete payment.
    /// This value drives the order's ExpiresAt (via NumbersReservedEvent), so it must be
    /// long enough to cover slow crypto-network confirmations before the payment webhook arrives.
    /// </summary>
    public int ReservationMinutes { get; set; } = 120;
}
