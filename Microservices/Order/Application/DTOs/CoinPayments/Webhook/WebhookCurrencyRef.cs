using System.Text.Json.Serialization;

namespace CryptoJackpot.Order.Application.DTOs.CoinPayments.Webhook;

public class WebhookCurrencyRef
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("smartContract")]
    public string? SmartContract { get; set; }
}

