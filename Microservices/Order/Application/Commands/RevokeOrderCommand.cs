using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Commands;

/// <summary>
/// Command to revoke an order that was optimistically completed on InvoicePending.
/// Used when CoinPayments later signals payment failure (InvoiceTimedOut, InvoiceCancelled,
/// or InvoiceCompleted with insufficient amount).
/// Revokes tickets (sets to Refunded), cancels order, and publishes OrderRevokedEvent.
/// </summary>
public class RevokeOrderCommand : IRequest<Result>
{
    public Guid OrderId { get; set; }
    public long UserId { get; set; }
    public string Reason { get; set; } = null!;
}

