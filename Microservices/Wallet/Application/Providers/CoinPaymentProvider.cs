using System.Net;
using System.Security.Cryptography;
using System.Text;
using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Wallet.Domain.Interfaces;

namespace CryptoJackpot.Wallet.Application.Providers;

public class CoinPaymentProvider : ICoinPaymentProvider
{
    private const string ApiVersion = "1";
    
    private static long _lastNonce;
    private static readonly object _nonceLock = new();
    
    private readonly string _privateKey;
    private readonly string _publicKey;
    private readonly IHttpClientFactory _httpClientFactory;

    public CoinPaymentProvider(
        string privateKey, 
        string publicKey,
        IHttpClientFactory httpClientFactory)
    {
        _privateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
        _publicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<RestResponse> CallApiAsync(
        string command, 
        SortedList<string, string>? parms = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        
        var parameters = parms ?? new SortedList<string, string>();
        
        parameters["version"] = ApiVersion;
        parameters["key"] = _publicKey;
        parameters["command"] = command;
        parameters["nonce"] = GenerateNonce().ToString();

        var postData = BuildPostData(parameters);
        var hmacSignature = GenerateHmacSignature(postData);

        return await SendRequestAsync(postData, hmacSignature, cancellationToken);
    }

    private static string BuildPostData(SortedList<string, string> parameters)
    {
        var postDataBuilder = new StringBuilder();
        
        foreach (var pair in parameters)
        {
            if (postDataBuilder.Length > 0)
                postDataBuilder.Append('&');
            
            postDataBuilder.Append(pair.Key);
            postDataBuilder.Append('=');
            postDataBuilder.Append(Uri.EscapeDataString(pair.Value));
        }

        return postDataBuilder.ToString();
    }

    private static long GenerateNonce()
    {
        lock (_nonceLock)
        {
            // Usar segundos es más seguro ante reinicios y desajustes de reloj
            var currentNonce = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
            if (currentNonce <= _lastNonce)
                currentNonce = _lastNonce + 1;
        
            _lastNonce = currentNonce;
            return currentNonce;
        }
    }

    private string GenerateHmacSignature(string postData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_privateKey);
        var postBytes = Encoding.UTF8.GetBytes(postData);
        
        using var hmacSha512 = new HMACSHA512(keyBytes);
        var hash = hmacSha512.ComputeHash(postBytes);
        
        return Convert.ToHexString(hash);
    }

    private async Task<RestResponse> SendRequestAsync(
        string postData, 
        string hmacSignature,
        CancellationToken cancellationToken)
    {
        var restResponse = new RestResponse();

        try
        {
            using var httpClient = _httpClientFactory.CreateClient("CoinPayments");
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, (string?)null);
            
            requestMessage.Content = new StringContent(
                postData, 
                Encoding.UTF8, 
                "application/x-www-form-urlencoded");
            
            requestMessage.Headers.Add("HMAC", hmacSignature);

            using var response = await httpClient.SendAsync(requestMessage, cancellationToken);

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
}