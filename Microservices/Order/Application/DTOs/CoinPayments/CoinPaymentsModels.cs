using System.Text.Json.Serialization;

namespace CryptoJackpot.Order.Application.DTOs.CoinPayments;

public class CoinPaymentsApiResponse<T> where T : class
{
    [JsonPropertyName("invoices")]
    public List<T>? Invoices { get; init; }

    [JsonIgnore]
    public bool IsSuccess { get; set; }

    [JsonIgnore]
    public string Error { get; set; } = string.Empty;

    [JsonIgnore]
    public T? FirstResult => Invoices?.FirstOrDefault();
}

public class CreateInvoiceRequest
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<InvoiceItem> Items { get; set; } = new();

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("customData")]
    public string? CustomData { get; set; }

    [JsonPropertyName("webhookData")]
    public InvoiceWebhookData? WebhookData { get; set; }
}

public class InvoiceItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public InvoiceItemQuantity Quantity { get; set; } = new();

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
}

public class InvoiceItemQuantity
{
    [JsonPropertyName("value")]
    public int Value { get; set; } = 1;

    [JsonPropertyName("type")]
    public int Type { get; set; } = 2;
}

public class InvoiceWebhookData
{
    [JsonPropertyName("notificationsUrl")]
    public string NotificationsUrl { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public Dictionary<string, string>? Params { get; set; }
}

public class CreateInvoiceResult
{
    [JsonPropertyName("id")]
    public string InvoiceId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("statusUrl")]
    public string StatusUrl { get; set; } = string.Empty;

    [JsonPropertyName("checkoutUrl")]
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
