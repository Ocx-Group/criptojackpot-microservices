namespace CryptoJackpot.Notification.Application.Interfaces;

/// <summary>
/// Result of an email send operation with details about success or failure.
/// </summary>
public record EmailSendResult(bool Success, string? ErrorMessage = null);

public interface IEmailProvider
{
    Task<EmailSendResult> SendEmailAsync(string to, string subject, string body);
}
