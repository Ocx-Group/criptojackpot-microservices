using System.Text.Json.Serialization;

namespace CryptoJackpot.Order.Application.DTOs.CoinPayments;

/// <summary>
/// Response from CoinPayments when registering a webhook.
/// </summary>
public class RegisterWebhookResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("notificationsUrl")]
    public string NotificationsUrl { get; set; } = string.Empty;

    [JsonPropertyName("notifications")]
    public List<string> Notifications { get; set; } = new();
}

