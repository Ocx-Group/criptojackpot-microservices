using CryptoJackpot.Notification.Application.Configuration;
using CryptoJackpot.Notification.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CryptoJackpot.Notification.Application.Providers;

public class SmtpEmailProvider : IEmailProvider
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailProvider> _logger;

    public SmtpEmailProvider(IOptions<NotificationConfiguration> config, ILogger<SmtpEmailProvider> logger)
    {
        _settings = config.Value.Smtp ?? throw new InvalidOperationException("SMTP settings are not configured.");
        _logger = logger;
    }

    public async Task<EmailSendResult> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port);
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            client.EnableSsl = _settings.UseSsl;

            var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {To}", to);
            return new EmailSendResult(true);
        }
        catch (Exception ex)
        {
            var errorMessage = $"SMTP error: {ex.Message}";
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return new EmailSendResult(false, errorMessage);
        }
    }
}
