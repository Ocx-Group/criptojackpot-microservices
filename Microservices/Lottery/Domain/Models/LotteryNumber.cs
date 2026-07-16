using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Lottery.Domain.Enums;

namespace CryptoJackpot.Lottery.Domain.Models;

public class LotteryNumber : BaseEntity
{
    public Guid LotteryNumberGuid { get; set; } = Guid.NewGuid();
    public long LotteryId { get; set; }
    public int Number { get; set; }

    /// <summary>
    /// Zero-padded display representation of Number, persisted at generation time
    /// (e.g. "007" for Pick3, "0007" for a 0-9999 raffle). This exact string is
    /// what users see everywhere (board, cart, orders, tickets, winners, emails).
    /// </summary>
    public string DisplayNumber { get; set; } = null!;

    public int Series { get; set; }
    public NumberStatus Status { get; set; }
    
    /// <summary>
    /// Computed property: true if Status is Available
    /// </summary>
    public bool IsAvailable => Status == NumberStatus.Available;
    
    /// <summary>
    /// Order ID that reserved this number (during checkout)
    /// </summary>
    public Guid? OrderId { get; set; }
    
    /// <summary>
    /// Ticket ID that purchased this number (after payment)
    /// </summary>
    public Guid? TicketId { get; set; }
    
    /// <summary>
    /// When the reservation expires (for pending orders)
    /// </summary>
    public DateTime? ReservationExpiresAt { get; set; }

    // Navigation
    public virtual LotteryDraw Lottery { get; set; } = null!;
}