using System.Text.Json;
using System.Text.Json.Serialization;
using CryptoJackpot.Order.Application.Converters;

namespace CryptoJackpot.Order.Application.DTOs.CoinPayments.Webhook;

public class WebhookPayment
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("invoiceId")]
    public string? InvoiceId { get; set; }

    [JsonPropertyName("createdAt")]
    public long? CreatedAt { get; set; }

    [JsonPropertyName("expiresAt")]
    public long? ExpiresAt { get; set; }

    [JsonPropertyName("detectedAt")]
    public long? DetectedAt { get; set; }

    [JsonPropertyName("pendingAt")]
    public long? PendingAt { get; set; }

    [JsonPropertyName("confirmedAt")]
    public long? ConfirmedAt { get; set; }

    [JsonPropertyName("completedAt")]
    public long? CompletedAt { get; set; }

    /// <summary>
    /// Payment state: e.g. "Confirmed", "Completed", etc.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("isGuest")]
    public bool? IsGuest { get; set; }

    [JsonPropertyName("hotWallet")]
    public WebhookHotWallet? HotWallet { get; set; }

    [JsonPropertyName("payout")]
    [JsonConverter(typeof(RawJsonConverter))]
    public JsonElement? Payout { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }
}

