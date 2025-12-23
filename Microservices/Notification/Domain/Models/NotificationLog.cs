using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Notification.Domain.Models;

public class NotificationLog : BaseEntity
{
    public long Id { get; set; }
    public string Type { get; set; } = null!;
    public string Recipient { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string? TemplateName { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
}
