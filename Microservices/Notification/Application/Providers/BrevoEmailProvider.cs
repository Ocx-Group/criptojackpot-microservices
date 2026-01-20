using CryptoJackpot.Notification.Application.Configuration;
using CryptoJackpot.Notification.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CryptoJackpot.Notification.Application.Providers;

/// <summary>
/// Email provider that uses Brevo (formerly Sendinblue) API to send transactional emails.
/// </summary>
public class BrevoEmailProvider : IEmailProvider
{
    private readonly BrevoSettings _settings;
    private readonly ILogger<BrevoEmailProvider> _logger;
    private readonly HttpClient _httpClient;
    private const string BrevoApiUrl = "https://api.brevo.com/v3/smtp/email";

    public BrevoEmailProvider(
        IOptions<NotificationConfiguration> config,
        ILogger<BrevoEmailProvider> logger,
        HttpClient httpClient)
    {
        _settings = config.Value.Brevo ?? throw new InvalidOperationException("Brevo settings are not configured.");
        _logger = logger;
        _httpClient = httpClient;
        
        // Configure the HttpClient with the API key
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("api-key", _settings.ApiKey);
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var emailPayload = new
            {
                sender = new
                {
                    name = _settings.SenderName,
                    email = _settings.SenderEmail
                },
                to = new[]
                {
                    new { email = to }
                },
                subject = subject,
                htmlContent = body
            };

            var jsonContent = JsonSerializer.Serialize(emailPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(BrevoApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully via Brevo to {To}", to);
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send email via Brevo to {To}. Status: {StatusCode}, Response: {Response}",
                to, response.StatusCode, responseContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending email via Brevo to {To}", to);
            return false;
        }
    }
}
