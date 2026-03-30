using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Order;

/// <summary>
/// Integration event published when an order is completed (payment successful).
/// Consumed by: Lottery microservice (to mark numbers as sold permanently)
///              Notification microservice (to send purchase confirmation email)
/// </summary>
public class OrderCompletedEvent : Event
{
    public Guid OrderId { get; set; }
    public Guid TicketId { get; set; }
    public Guid LotteryId { get; set; }
    public long UserId { get; set; }
    
    /// <summary>Buyer's UserGuid (from JWT sub claim) for cross-service identity resolution.</summary>
    public Guid BuyerUserGuid { get; set; }
    
    public List<Guid> LotteryNumberIds { get; set; } = [];
    public string TransactionId { get; set; } = null!;
    
    // ── Notification data ───────────────────────────────────────────────
    
    /// <summary>Buyer's email for purchase confirmation</summary>
    public string UserEmail { get; set; } = null!;
    
    /// <summary>Buyer's display name</summary>
    public string UserName { get; set; } = null!;
    
    /// <summary>Lottery title snapshot</summary>
    public string LotteryTitle { get; set; } = null!;
    
    /// <summary>Lottery number identifier (e.g. "001")</summary>
    public string LotteryNo { get; set; } = null!;
    
    /// <summary>Total amount paid</summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>Purchased ticket details for the confirmation email</summary>
    public List<PurchasedTicketItem> Tickets { get; set; } = [];

    /// <summary>Lottery type for number formatting (e.g., Pick3 → 3-digit display)</summary>
    public int LotteryType { get; set; }
}

/// <summary>
/// Represents a single purchased ticket within an OrderCompletedEvent.
/// </summary>
public class PurchasedTicketItem
{
    public int Number { get; set; }
    public int Series { get; set; }
    public decimal Amount { get; set; }
    public bool IsGift { get; set; }
    public long? GiftRecipientId { get; set; }
}

