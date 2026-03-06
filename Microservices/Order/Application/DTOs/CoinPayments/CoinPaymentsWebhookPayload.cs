using System.Text.Json.Serialization;

namespace CryptoJackpot.Order.Application.DTOs.CoinPayments;

/// <summary>
/// Webhook payload sent by CoinPayments for invoice events.
/// </summary>
public class CoinPaymentsWebhookPayload
{
    /// <summary>
    /// The CoinPayments ID for the invoice
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The notification type.
    /// Enum: invoiceCreated, invoicePending, invoicePaid, invoiceCompleted,
    ///       invoiceCancelled, invoiceTimedOut, invoicePaymentCreated, invoicePaymentTimedOut.
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

public class WebhookInvoice
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("invoiceId")]
    public string? InvoiceId { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("merchantId")]
    public string? MerchantId { get; set; }

    [JsonPropertyName("merchantClientId")]
    public string? MerchantClientId { get; set; }

    [JsonPropertyName("invoiceNumber")]
    public string? InvoiceNumber { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("expiresDate")]
    public string? ExpiresDate { get; set; }

    [JsonPropertyName("customData")]
    public Dictionary<string, object>? CustomData { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("amount")]
    public WebhookInvoiceAmount? Amount { get; set; }

    [JsonPropertyName("payments")]
    public List<WebhookPayment>? Payments { get; set; }

    [JsonPropertyName("canceledAt")]
    public long? CanceledAt { get; set; }

    [JsonPropertyName("completedAt")]
    public long? CompletedAt { get; set; }

    [JsonPropertyName("confirmedAt")]
    public long? ConfirmedAt { get; set; }
}

public class WebhookInvoiceAmount
{
    [JsonPropertyName("currencyId")]
    public string? CurrencyId { get; set; }

    [JsonPropertyName("displayValue")]
    public string? DisplayValue { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("total")]
    public string? Total { get; set; }
}

public class WebhookPayment
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("paymentId")]
    public string? PaymentId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("txId")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("amount")]
    public WebhookInvoiceAmount? Amount { get; set; }

    [JsonPropertyName("confirmedAt")]
    public long? ConfirmedAt { get; set; }
}

