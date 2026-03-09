using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoJackpot.Order.Application.Converters;

/// <summary>
/// Converter that allows accepting any JSON token into a JsonElement without failing.
/// Useful for fields like customData or payout whose structure may vary.
/// </summary>
public class RawJsonConverter : JsonConverter<JsonElement?>
{
    public override JsonElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonElement.ParseValue(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, JsonElement? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            value.Value.WriteTo(writer);
        else
            writer.WriteNullValue();
    }
}

