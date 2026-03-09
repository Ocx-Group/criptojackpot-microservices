using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoJackpot.Order.Application.Converters;

/// <summary>
/// Centralized <see cref="JsonSerializerOptions"/> instances used across the Order service.
/// Each instance is static and thread-safe — do not mutate after initialization.
/// </summary>
public static class JsonDefaults
{
    /// <summary>
    /// Options for deserializing CoinPayments API responses.
    /// Case-insensitive property matching.
    /// </summary>
    public static JsonSerializerOptions ApiResponse { get; } = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Options for serializing requests sent to the CoinPayments API.
    /// CamelCase naming, nulls omitted.
    /// </summary>
    public static JsonSerializerOptions ApiRequest { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Options for deserializing CoinPayments webhook payloads.
    /// Case-insensitive + allows reading numbers that arrive as strings.
    /// </summary>
    public static JsonSerializerOptions Webhook { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };
}

