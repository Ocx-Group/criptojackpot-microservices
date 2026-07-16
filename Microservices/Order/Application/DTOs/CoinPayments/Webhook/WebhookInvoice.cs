using System.Text.Json;
using System.Text.Json.Serialization;
using CryptoJackpot.Order.Application.Converters;

namespace CryptoJackpot.Order.Application.DTOs.CoinPayments.Webhook;

public class WebhookInvoice
{
    /// <summary>
    /// The CoinPayments invoice UUID. Example: "814162d4-9de5-4653-9fbd-26e9d067df0f"
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The merchant's invoice ID / number. Example: "0007"
    /// </summary>
    [JsonPropertyName("invoiceId")]
    public string? InvoiceId { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("userEmail")]
    public string? UserEmail { get; set; }

    [JsonPropertyName("merchantId")]
    public string? MerchantId { get; set; }

    [JsonPropertyName("merchantClientId")]
    public string? MerchantClientId { get; set; }

    [JsonPropertyName("invoiceNumber")]
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Unix timestamp (seconds) when the invoice was created.
    /// CoinPayments sends this as a number, not a string.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public long? CreatedAt { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Unix timestamp (seconds) when the invoice expires.
    /// CoinPayments sends this as a number, not a string.
    /// </summary>
    [JsonPropertyName("expiresDate")]
    public long? ExpiresDate { get; set; }

    [JsonPropertyName("customData")]
    [JsonConverter(typeof(RawJsonConverter))]
    public JsonElement? CustomData { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("amount")]
    public WebhookInvoiceAmount? Amount { get; set; }

    [JsonPropertyName("lineItems")]
    public List<WebhookLineItem>? LineItems { get; set; }

    [JsonPropertyName("payments")]
    public List<WebhookPayment>? Payments { get; set; }

    [JsonPropertyName("canceledAt")]
    public long? CanceledAt { get; set; }

    [JsonPropertyName("completedAt")]
    public long? CompletedAt { get; set; }

    [JsonPropertyName("confirmedAt")]
    public long? ConfirmedAt { get; set; }
}

