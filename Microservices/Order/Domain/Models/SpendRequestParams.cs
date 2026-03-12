namespace CryptoJackpot.Order.Domain.Models;

/// <summary>
/// Parameters for creating a spend request (withdrawal or conversion) from a merchant wallet.
/// </summary>
public class SpendRequestParams
{
    /// <summary>Address which client wants to send funds to.</summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// ID of the currency of the spend destination.
    /// For non-conversion spends this equals the wallet's currency.
    /// </summary>
    public string ToCurrency { get; set; } = string.Empty;

    /// <summary>Amount as decimal string.</summary>
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// Optional currency of <see cref="Amount"/> in {CurrencyId}:{ContractAddress} format.
    /// </summary>
    public string? AmountCurrency { get; set; }

    /// <summary>
    /// Optional override for the blockchain fee (within 10 % of the system suggestion).
    /// </summary>
    public string? BlockchainFeeOverride { get; set; }

    /// <summary>Optional decimal value of the blockchain fee override.</summary>
    public double? BlockchainFeeOverrideDecimal { get; set; }

    /// <summary>Optional user-defined note for the spend.</summary>
    public string? Memo { get; set; }

    /// <summary>
    /// When <c>true</c> the receiver absorbs the fee; when <c>false</c> (default)
    /// fees are added on top and deducted from the sender balance.
    /// </summary>
    public bool? ReceiverPaysFee { get; set; }
}
