using System.Text.Json.Serialization;

namespace CryptoJackpot.Order.Application.DTOs.CoinPayments.Webhook;

/// <summary>
/// Webhook payload sent by CoinPayments for invoice events.
/// </summary>
public class CoinPaymentsWebhookPayload
{
    /// <summary>
    /// The CoinPayments webhook event ID (not the invoice ID).
    /// Example: "e8c89d7b9b844adebf2b157fb7b00d8e"
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The notification type.
    /// CoinPayments sends PascalCase: InvoicePaid, InvoiceCompleted, etc.
    /// Use case-insensitive comparison as recommended by CoinPayments.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Date and time of producing the event (UTC ISO-8601)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    /// <summary>
    /// Details of the invoice to which refers the webhook notification
    /// </summary>
    [JsonPropertyName("invoice")]
    public WebhookInvoice? Invoice { get; set; }
}

