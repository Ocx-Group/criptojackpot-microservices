using System.Text.Json.Serialization;

namespace CryptoJackpot.Order.Application.DTOs.CoinPayments;

public class CoinPaymentsApiResponse<T> where T : class
{
    [JsonPropertyName("invoices")]
    public List<T>? Invoices { get; init; }

    [JsonPropertyName("items")]
    public List<T>? Items { get; init; }

    [JsonIgnore]
    public bool IsSuccess { get; set; }

    [JsonIgnore]
    public string Error { get; set; } = string.Empty;

    [JsonIgnore]
    public T? FirstResult => Invoices?.FirstOrDefault() ?? Items?.FirstOrDefault();
}

// ── Currencies (GET /api/v2/currencies) — PUBLIC, no auth ───────────

public class CurrencyResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("logo")]
    public CurrencyLogo? Logo { get; set; }

    [JsonPropertyName("decimalPlaces")]
    public int DecimalPlaces { get; set; }

    [JsonPropertyName("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("capabilities")]
    public List<string>? Capabilities { get; set; }

    [JsonPropertyName("requiredConfirmations")]
    public int RequiredConfirmations { get; set; }

    [JsonPropertyName("isEnabledForPayment")]
    public bool IsEnabledForPayment { get; set; }
}

public class CurrencyLogo
{
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;
}

// ── Invoice creation request ────────────────────────────────────────

public class CreateInvoiceRequest
{
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<InvoiceItem> Items { get; set; } = new();

    [JsonPropertyName("amount")]
    public InvoiceAmount Amount { get; set; } = new();

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("invoiceId")]
    public string? InvoiceId { get; set; }

    [JsonPropertyName("webhooks")]
    public List<InvoiceWebhook>? Webhooks { get; set; }

    [JsonPropertyName("customData")]
    public Dictionary<string, string>? CustomData { get; set; }

    [JsonPropertyName("successUrl")]
    public string? SuccessUrl { get; set; }

    [JsonPropertyName("cancelUrl")]
    public string? CancelUrl { get; set; }
}

public class InvoiceItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public InvoiceItemQuantity Quantity { get; set; } = new();

    [JsonPropertyName("originalAmount")]
    public string? OriginalAmount { get; set; }

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
}

public class InvoiceItemQuantity
{
    [JsonPropertyName("value")]
    public int Value { get; set; } = 1;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "2";
}

public class InvoiceAmount
{
    [JsonPropertyName("breakdown")]
    public InvoiceAmountBreakdown? Breakdown { get; set; }

    [JsonPropertyName("total")]
    public string Total { get; set; } = string.Empty;
}

public class InvoiceAmountBreakdown
{
    [JsonPropertyName("subtotal")]
    public string Subtotal { get; set; } = string.Empty;
}

public class InvoiceWebhook
{
    [JsonPropertyName("notificationsUrl")]
    public string NotificationsUrl { get; set; } = string.Empty;

    [JsonPropertyName("notifications")]
    public List<string> Notifications { get; set; } = new();
}

// ── Invoice creation response ───────────────────────────────────────

public class CreateInvoiceResult
{
    [JsonPropertyName("id")]
    public string InvoiceId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("link")]
    public string StatusUrl { get; set; } = string.Empty;

    [JsonPropertyName("checkoutLink")]
    public string CheckoutUrl { get; set; } = string.Empty;

    [JsonPropertyName("qrCodeUrl")]
    public string QrCodeUrl { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public InvoiceAmountResponse? Amount { get; set; }

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public string ExpiresAt { get; set; } = string.Empty;
}

public class InvoiceAmountResponse
{
    [JsonPropertyName("currencyId")]
    public string CurrencyId { get; set; } = string.Empty;

    [JsonPropertyName("displayValue")]
    public string DisplayValue { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

// ── Spend request (POST /merchant/wallets/:id/spend/request) ────────

/// <summary>
/// Request body for creating a spend request (withdrawal or conversion) from a merchant wallet.
/// </summary>
public class CreateSpendRequest
{
    /// <summary>
    /// Address which client wants to send funds to.
    /// </summary>
    [JsonPropertyName("toAddress")]
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// ID of the currency of the spend destination.
    /// For anything except conversion it is equal to the wallet's currency.
    /// </summary>
    [JsonPropertyName("toCurrency")]
    public string ToCurrency { get; set; } = string.Empty;

    /// <summary>
    /// The amount of money as decimal to send to the recipient address.
    /// </summary>
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// Currency of the amount field in {CurrencyId}:{ContractAddress} format.
    /// Optional — when omitted the wallet's native currency is assumed.
    /// </summary>
    [JsonPropertyName("amountCurrency")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AmountCurrency { get; set; }

    /// <summary>
    /// Overrides the system-suggested blockchain fee (within 10 % range).
    /// </summary>
    [JsonPropertyName("blockchainFeeOverride")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BlockchainFeeOverride { get; set; }

    /// <summary>
    /// Decimal representation of the blockchain fee override.
    /// </summary>
    [JsonPropertyName("blockchainFeeOverrideDecimal")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? BlockchainFeeOverrideDecimal { get; set; }

    /// <summary>
    /// Optional user-defined note for the spend.
    /// </summary>
    [JsonPropertyName("memo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Memo { get; set; }

    /// <summary>
    /// When true the receiver pays the fee; when false (default) fees are added on top
    /// and deducted from the sender balance.
    /// </summary>
    [JsonPropertyName("receiverPaysFee")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ReceiverPaysFee { get; set; }
}

/// <summary>
/// Response from the create spend request endpoint — a preview of the pending transaction.
/// Call the confirm endpoint with <see cref="SpendRequestId"/> to submit to the blockchain.
/// </summary>
public class SpendRequestResult
{
    /// <summary>
    /// ID of this spend request. Pass this to the confirm endpoint.
    /// </summary>
    [JsonPropertyName("spendRequestId")]
    public string SpendRequestId { get; set; } = string.Empty;

    [JsonPropertyName("fromWalletId")]
    public string FromWalletId { get; set; } = string.Empty;

    [JsonPropertyName("toAddress")]
    public string ToAddress { get; set; } = string.Empty;

    [JsonPropertyName("fromCurrencyId")]
    public string FromCurrencyId { get; set; } = string.Empty;

    [JsonPropertyName("fromCurrencySymbol")]
    public string FromCurrencySymbol { get; set; } = string.Empty;

    /// <summary>
    /// Amount of funds being spent (from the sender's perspective).
    /// </summary>
    [JsonPropertyName("fromAmount")]
    public string FromAmount { get; set; } = string.Empty;

    [JsonPropertyName("toCurrencyId")]
    public string ToCurrencyId { get; set; } = string.Empty;

    [JsonPropertyName("toCurrencySymbol")]
    public string ToCurrencySymbol { get; set; } = string.Empty;

    /// <summary>
    /// Amount of funds being received at the destination.
    /// </summary>
    [JsonPropertyName("toAmount")]
    public string ToAmount { get; set; } = string.Empty;

    /// <summary>
    /// Blockchain fee required to perform the transfer.
    /// </summary>
    [JsonPropertyName("blockchainFee")]
    public string BlockchainFee { get; set; } = string.Empty;

    /// <summary>
    /// Fee withheld by CoinPayments for the service.
    /// </summary>
    [JsonPropertyName("coinpaymentsFee")]
    public string CoinpaymentsFee { get; set; } = string.Empty;

    [JsonPropertyName("memo")]
    public string? Memo { get; set; }

    /// <summary>
    /// Populated when the spend request is a currency conversion.
    /// </summary>
    [JsonPropertyName("conversionPreview")]
    public object? ConversionPreview { get; set; }
}

// ── Spend confirmation (POST /merchant/wallets/:id/spend/confirmation) ──

/// <summary>
/// Body payload for confirming a pending spend request.
/// </summary>
public class ConfirmSpendRequest
{
    /// <summary>
    /// ID of the spend request returned by the create spend request endpoint.
    /// </summary>
    [JsonPropertyName("spendRequestId")]
    public string SpendRequestId { get; set; } = string.Empty;
}

