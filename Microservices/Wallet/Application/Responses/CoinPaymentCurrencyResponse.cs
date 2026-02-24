namespace CryptoJackpot.Wallet.Application.Responses;

/// <summary>
/// Represents a supported cryptocurrency returned by the CoinPayments API v2.
/// </summary>
public class CoinPaymentCurrencyResponse
{
    /// <summary>
    /// The numeric currency ID used by the CoinPayments API (e.g., "1002" for LTCT).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The ticker symbol of the currency (e.g., BTC, ETH, LTCT).
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// The human-readable name of the currency (e.g., "Bitcoin", "Ethereum").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of decimal places supported by the currency.
    /// </summary>
    public int DecimalPlaces { get; set; }

    /// <summary>
    /// Current status of the currency on the CoinPayments platform (e.g., "active").
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this currency can be used as a settlement currency.
    /// </summary>
    public bool IsSettlement { get; set; }

    /// <summary>
    /// Indicates whether this is a fiat currency.
    /// </summary>
    public bool IsFiat { get; set; }

    /// <summary>
    /// Current USD exchange rate for this currency.
    /// </summary>
    public string RateUsd { get; set; } = string.Empty;
}

