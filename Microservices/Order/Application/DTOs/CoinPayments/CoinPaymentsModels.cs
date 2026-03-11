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
