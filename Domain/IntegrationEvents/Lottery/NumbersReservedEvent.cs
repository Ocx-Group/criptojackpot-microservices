using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;

/// <summary>
/// Integration event published when numbers are reserved via SignalR Hub.
/// Consumed by: Order microservice (to create/update pending order)
/// </summary>
public class NumbersReservedEvent : Event
{
    /// <summary>
    /// Pre-generated OrderId to use (allows immediate response to client)
    /// </summary>
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// The lottery ID these numbers belong to
    /// </summary>
    public Guid LotteryId { get; set; }
    
    /// <summary>
    /// The user who reserved the numbers
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// Buyer's UserGuid (JWT sub claim) for cross-service identity resolution.
    /// </summary>
    public Guid UserGuid { get; set; }
    
    /// <summary>
    /// User's email for purchase confirmation notification
    /// </summary>
    public string UserEmail { get; set; } = null!;
    
    /// <summary>
    /// User's display name for purchase confirmation notification
    /// </summary>
    public string UserName { get; set; } = null!;
    
    /// <summary>
    /// Lottery title snapshot for notification context
    /// </summary>
    public string LotteryTitle { get; set; } = null!;
    
    /// <summary>
    /// Lottery number identifier snapshot (e.g. "001")
    /// </summary>
    public string LotteryNo { get; set; } = null!;
    
    /// <summary>
    /// The reserved lottery number IDs
    /// </summary>
    public List<Guid> LotteryNumberIds { get; set; } = [];
    
    /// <summary>
    /// The actual numbers reserved (e.g., [10, 10] for number 10 series 1 and 2)
    /// </summary>
    public int[] Numbers { get; set; } = [];
    
    /// <summary>
    /// The series of each reserved number
    /// </summary>
    public int[] SeriesArray { get; set; } = [];
    
    /// <summary>
    /// Price per ticket
    /// </summary>
    public decimal TicketPrice { get; set; }
    
    /// <summary>
    /// Total amount for this reservation
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// CoinPayments currency ID for payment (e.g. "2")
    /// </summary>
    public string CryptoCurrencyId { get; set; } = null!;
    
    /// <summary>
    /// Crypto ticker symbol for payment (e.g. "LTCT", "BTC")
    /// </summary>
    public string CryptoCurrencySymbol { get; set; } = null!;
    
    /// <summary>
    /// When the reservation expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// If true, update existing pending order. If false, create new order.
    /// </summary>
    public bool IsAddToExistingOrder { get; set; }
    
    /// <summary>
    /// Existing order ID to update (if IsAddToExistingOrder is true)
    /// </summary>
    public Guid? ExistingOrderId { get; set; }

    /// <summary>
    /// Lottery type for number formatting in downstream services (e.g., Pick3 → 3-digit display)
    /// </summary>
    public int LotteryType { get; set; }
}
