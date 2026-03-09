using System.Text.Json.Serialization;

namespace CryptoJackpot.Order.Application.DTOs.CoinPayments.Webhook;

public class WebhookInvoiceAmount
{
    [JsonPropertyName("currency")]
    public WebhookCurrencyRef? Currency { get; set; }

    [JsonPropertyName("subtotal")]
    public decimal? Subtotal { get; set; }

    [JsonPropertyName("shippingTotal")]
    public decimal? ShippingTotal { get; set; }

    [JsonPropertyName("discountTotal")]
    public decimal? DiscountTotal { get; set; }

    [JsonPropertyName("taxTotal")]
    public decimal? TaxTotal { get; set; }

    [JsonPropertyName("total")]
    public decimal? Total { get; set; }
}

