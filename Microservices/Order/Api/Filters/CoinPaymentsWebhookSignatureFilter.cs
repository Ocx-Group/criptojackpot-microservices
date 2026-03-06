using System.Security.Cryptography;
using System.Text;
using CryptoJackpot.Order.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CryptoJackpot.Order.Api.Filters;

/// <summary>
/// Action filter that validates the CoinPayments webhook signature.
/// Verifies the X-CoinPayments-Signature header using HMAC-SHA256 with the webhook secret.
/// Optionally validates the sender IP address against CoinPayments known IPs.
/// </summary>
public class CoinPaymentsWebhookSignatureFilter : IAsyncActionFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CoinPaymentsWebhookSignatureFilter> _logger;

    private const string SignatureHeader = "X-CoinPayments-Signature";
    private const string ClientHeader = "X-CoinPayments-Client";
    private const string TimestampHeader = "X-CoinPayments-Timestamp";

    /// <summary>
    /// Known CoinPayments webhook IP addresses.
    /// Instance A: hook1.coinpayments.com and hook2.coinpayments.com
    /// Instance B: 23.183.244.98
    /// </summary>
    private static readonly HashSet<string> AllowedIpAddresses = new()
    {
        "23.183.244.249", // hook1.coinpayments.com (instance a)
        "23.183.244.250", // hook2.coinpayments.com (instance a)
        "23.183.244.98"   // instance b
    };

    public CoinPaymentsWebhookSignatureFilter(
        IConfiguration configuration,
        ILogger<CoinPaymentsWebhookSignatureFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        // 1. Validate IP address (optional, configurable)
        var validateIp = _configuration.GetValue("CoinPayments:ValidateWebhookIp", false);
        if (validateIp)
        {
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            if (remoteIp is null || !AllowedIpAddresses.Contains(remoteIp))
            {
                _logger.LogWarning(
                    "CoinPayments webhook rejected: IP {RemoteIp} is not in the allowed list",
                    remoteIp);
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    message = "Unauthorized webhook source"
                });
                return;
            }
        }

        // 2. Extract required headers
        if (!request.Headers.TryGetValue(SignatureHeader, out var signatureHeader) ||
            string.IsNullOrWhiteSpace(signatureHeader))
        {
            _logger.LogWarning("CoinPayments webhook rejected: Missing {Header} header", SignatureHeader);
            context.Result = new UnauthorizedObjectResult(new
            {
                success = false,
                message = $"Missing {SignatureHeader} header"
            });
            return;
        }

        request.Headers.TryGetValue(ClientHeader, out var clientId);
        request.Headers.TryGetValue(TimestampHeader, out var timestamp);

        // 3. Read the raw request body
        request.EnableBuffering();
        request.Body.Position = 0;

        string body;
        using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
        }

        request.Body.Position = 0;

        // 4. Get the webhook secret (falls back to ClientSecret if WebhookSecret not configured)
        var webhookSecret = _configuration[CoinPaymentsConfigKeys.WebhookSecret]
                            ?? _configuration[CoinPaymentsConfigKeys.ClientSecret];

        if (string.IsNullOrEmpty(webhookSecret))
        {
            _logger.LogError("CoinPayments webhook secret is not configured");
            context.Result = new ObjectResult(new
            {
                success = false,
                message = "Webhook validation configuration error"
            })
            {
                StatusCode = 500
            };
            return;
        }

        // 5. Compute HMAC-SHA256 signature
        // CoinPayments signature format: \ufeff{METHOD}{URL}{clientId}{timestamp}{body}
        var method = request.Method.ToUpperInvariant();
        var fullUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

        var message = $"\ufeff{method}{fullUrl}{clientId}{timestamp}{body}";

        var keyBytes = Encoding.UTF8.GetBytes(webhookSecret);
        var msgBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var computedHash = hmac.ComputeHash(msgBytes);
        var computedSignature = Convert.ToBase64String(computedHash);

        // 6. Constant-time comparison to prevent timing attacks
        var expectedBytes = Encoding.UTF8.GetBytes(computedSignature);
        var receivedBytes = Encoding.UTF8.GetBytes(signatureHeader.ToString());

        if (!CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes))
        {
            _logger.LogWarning(
                "CoinPayments webhook signature validation failed. ClientId: {ClientId}, Timestamp: {Timestamp}",
                clientId.ToString(), timestamp.ToString());
            context.Result = new UnauthorizedObjectResult(new
            {
                success = false,
                message = "Invalid webhook signature"
            });
            return;
        }

        _logger.LogDebug(
            "CoinPayments webhook signature validated successfully. ClientId: {ClientId}",
            clientId.ToString());

        await next();
    }
}

