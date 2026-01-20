using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Notification;

/// <summary>
/// Individual email job event for sending a single marketing email.
/// Used to distribute email sending across multiple workers.
/// </summary>
public class SendMarketingEmailEvent : Event
{
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string UserLastName { get; set; } = null!;
    
    // Lottery information
    public Guid LotteryId { get; set; }
    public string LotteryTitle { get; set; } = null!;
    public string LotteryDescription { get; set; } = null!;
    public decimal TicketPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxTickets { get; set; }
    
    // Tracking
    public Guid CampaignId { get; set; }
    public int BatchNumber { get; set; }
}
