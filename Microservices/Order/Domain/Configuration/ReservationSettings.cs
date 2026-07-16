namespace CryptoJackpot.Order.Domain.Configuration;

/// <summary>
/// Configuration for order payment timing. Bound from the "ReservationSettings" section,
/// shared with the Lottery service via the same ConfigMap so a single knob controls the
/// whole payment window.
/// </summary>
public class ReservationSettings
{
    public const string SectionName = "ReservationSettings";

    /// <summary>
    /// Minutes granted to complete payment. Also used to re-extend an order that had already
    /// expired when a late payment webhook (slow crypto network) arrives.
    /// </summary>
    public int ReservationMinutes { get; set; } = 120;
}
