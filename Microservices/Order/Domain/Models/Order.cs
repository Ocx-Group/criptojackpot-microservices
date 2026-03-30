using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Order.Domain.Enums;

namespace CryptoJackpot.Order.Domain.Models;

/// <summary>
/// Representa un intento de compra / carrito.
/// Tiene un countdown de 65 minutos para completar el pago.
/// </summary>
public class Order : BaseEntity
{
    /// <summary>
    /// External GUID for API exposure and cross-service communication
    /// </summary>
    public Guid OrderGuid { get; set; } = Guid.NewGuid();
    
    public long UserId { get; set; }
    
    /// <summary>
    /// Buyer's UserGuid (JWT sub claim) for cross-service identity resolution.
    /// </summary>
    public Guid UserGuid { get; set; }
    
    public Guid LotteryId { get; set; }
    
    /// <summary>
    /// Buyer's email snapshot (for notification after webhook-based payment)
    /// </summary>
    public string UserEmail { get; set; } = null!;
    
    /// <summary>
    /// Buyer's display name snapshot
    /// </summary>
    public string UserName { get; set; } = null!;
    
    /// <summary>
    /// Lottery title snapshot at order creation time
    /// </summary>
    public string LotteryTitle { get; set; } = null!;
    
    /// <summary>
    /// Lottery number identifier snapshot (e.g. "001")
    /// </summary>
    public string LotteryNo { get; set; } = null!;
    
    /// <summary>
    /// Lottery type snapshot for number formatting (e.g., Pick3 = 5 → 3-digit display)
    /// </summary>
    public int LotteryType { get; set; }
    
    public OrderStatus Status { get; set; }
    
    /// <summary>
    /// CoinPayments invoice ID associated with this order
    /// </summary>
    public string? InvoiceId { get; set; }
    
    /// <summary>
    /// Expiration time for pending orders (65 minutes from creation)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Computed property to check if order is expired
    /// </summary>
    public bool IsExpired => Status == OrderStatus.Pending && DateTime.UtcNow > ExpiresAt;
    
    /// <summary>
    /// Total amount calculated from order details
    /// </summary>
    public decimal TotalAmount => OrderDetails.Sum(d => d.Subtotal);
    
    /// <summary>
    /// Total number of items in the order
    /// </summary>
    public int TotalItems => OrderDetails.Sum(d => d.Quantity);
    
    // Navigation properties
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

