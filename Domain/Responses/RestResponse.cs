using System.Net;
using System.Text.Json;

namespace CryptoJackpot.Domain.Core.Responses;

public class RestResponse
{
    public string? Content { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string? StatusDescription { get; set; }
    
    public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode < 300;

    public T? Deserialize<T>(JsonSerializerOptions? options = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(Content))
            return null;

        options ??= new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<T>(Content, options);
    }

    public bool TryDeserialize<T>(out T? result, JsonSerializerOptions? options = null) where T : class
    {
        result = null;
        
        try
        {
            result = Deserialize<T>(options);
            return result != null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}