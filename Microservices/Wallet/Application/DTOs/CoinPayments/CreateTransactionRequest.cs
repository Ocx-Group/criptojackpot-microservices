namespace CryptoJackpot.Wallet.Application.DTOs.CoinPayments;

/// <summary>
/// Request parameters for creating a CoinPayments transaction
/// </summary>
public sealed record CreateTransactionRequest
{
    /// <summary>
    /// The amount of the transaction in the original currency
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// The original currency (e.g., USD, EUR)
    /// </summary>
    public required string CurrencyFrom { get; init; }

    /// <summary>
    /// The cryptocurrency to receive (e.g., BTC, ETH, LTCT)
    /// </summary>
    public required string CurrencyTo { get; init; }

    /// <summary>
    /// Optional buyer email for notifications
    /// </summary>
    public string? BuyerEmail { get; init; }

    /// <summary>
    /// Optional buyer name
    /// </summary>
    public string? BuyerName { get; init; }

    /// <summary>
    /// Optional item/product name
    /// </summary>
    public string? ItemName { get; init; }

    /// <summary>
    /// Optional IPN (Instant Payment Notification) callback URL
    /// </summary>
    public string? IpnUrl { get; init; }
}
