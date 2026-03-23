using CryptoJackpot.Domain.Core.Events;

namespace CryptoJackpot.Domain.Core.IntegrationEvents.Order;

/// <summary>
/// Integration event published when an order is revoked after optimistic completion.
/// This happens when CoinPayments sends InvoiceTimedOut, InvoiceCancelled, or InvoiceCompleted
/// with insufficient payment AFTER the order was already completed on InvoicePending.
/// Consumed by: Lottery microservice (to release sold numbers back to available and decrement SoldTickets)
/// </summary>
public class OrderRevokedEvent : Event
{
    public Guid OrderId { get; set; }
    public Guid LotteryId { get; set; }
    public long UserId { get; set; }
    public List<Guid> LotteryNumberIds { get; set; } = [];
    public string Reason { get; set; } = null!;
}

