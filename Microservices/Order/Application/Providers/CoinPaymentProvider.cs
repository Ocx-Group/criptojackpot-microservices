using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Order.Application.DTOs.CoinPayments;
using CryptoJackpot.Order.Domain.Constants;
using CryptoJackpot.Order.Domain.Interfaces;

namespace CryptoJackpot.Order.Application.Providers;

public class CoinPaymentProvider : ICoinPaymentProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _clientSecret;
    private readonly string _clientId;
    private readonly IHttpClientFactory _httpClientFactory;

    public CoinPaymentProvider(
        string clientSecret,
        string clientId,
        IHttpClientFactory httpClientFactory)
    {
        _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public Task<RestResponse> CreateInvoiceAsync(
        decimal amount,
        string currency,
        List<InvoiceLineItem> items,
        string? description = null,
        string? notificationsUrl = null,
        CancellationToken cancellationToken = default)
    {
        var invoiceItems = items.Select(item => new InvoiceItem
        {
            Name = item.Name,
            Quantity = new InvoiceItemQuantity
            {
                Value = item.Quantity,
                Type = 2
            },
            Amount = item.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
        }).ToList();

        var body = new CreateInvoiceRequest
        {
            ClientId = _clientId,
            Currency = currency,
            Items = invoiceItems,
            Description = description,
            WebhookData = !string.IsNullOrEmpty(notificationsUrl)
                ? new InvoiceWebhookData { NotificationsUrl = notificationsUrl }
                : null
        };

        return SendAsync(HttpMethod.Post, CoinPaymentsEndpoints.CreateInvoice, body, cancellationToken);
    }

    public Task<RestResponse> GetInvoiceAsync(string invoiceId, CancellationToken cancellationToken = default) =>
        SendAsync(
            HttpMethod.Get,
            string.Format(CoinPaymentsEndpoints.GetInvoiceById, invoiceId),
            null,
            cancellationToken);

    private async Task<RestResponse> SendAsync(
        HttpMethod method,
        string relativeEndpoint,
        object? body,
        CancellationToken cancellationToken)
    {
        var restResponse = new RestResponse();

        try
        {
            using var httpClient = _httpClientFactory.CreateClient(CoinPaymentsDefaults.HttpClientName);

            var baseAddress = httpClient.BaseAddress
                ?? throw new InvalidOperationException("CoinPayments HttpClient BaseAddress is not configured");

            var requestUri = new Uri(baseAddress, relativeEndpoint);

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

            var bodyJson = body is not null ? JsonSerializer.Serialize(body, JsonOptions) : string.Empty;

            var signature = BuildSignature(method.Method, requestUri.ToString(), timestamp, bodyJson);

            using var request = new HttpRequestMessage(method, requestUri);

            request.Headers.Add("X-CoinPayments-Client", _clientId);
            request.Headers.Add("X-CoinPayments-Timestamp", timestamp);
            request.Headers.Add("X-CoinPayments-Signature", signature);

            if (!string.IsNullOrEmpty(bodyJson))
                request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request, cancellationToken);

            restResponse.Content = await response.Content.ReadAsStringAsync(cancellationToken);
            restResponse.StatusCode = response.StatusCode;
            restResponse.StatusDescription = response.ReasonPhrase;
        }
        catch (OperationCanceledException)
        {
            restResponse.StatusCode = HttpStatusCode.RequestTimeout;
            restResponse.Content = "Request was cancelled";
            throw;
        }
        catch (HttpRequestException ex)
        {
            restResponse.StatusCode = HttpStatusCode.ServiceUnavailable;
            restResponse.Content = $"Error contacting CoinPayments API: {ex.Message}";
        }
        catch (Exception ex)
        {
            restResponse.StatusCode = HttpStatusCode.InternalServerError;
            restResponse.Content = $"Unexpected error: {ex.Message}";
            throw;
        }

        return restResponse;
    }

    private string BuildSignature(string httpMethod, string fullUrl, string timestamp, string body)
    {
        var message = $"\ufeff{httpMethod}{fullUrl}{_clientId}{timestamp}{body}";

        var keyBytes = Encoding.UTF8.GetBytes(_clientSecret);
        var msgBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(msgBytes);

        return Convert.ToBase64String(hash);
    }
}
