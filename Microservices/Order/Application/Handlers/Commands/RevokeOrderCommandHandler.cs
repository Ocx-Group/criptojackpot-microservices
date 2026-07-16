using System.Text.Json;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Audit;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

/// <summary>
/// Handles revocation of an order that was optimistically completed on InvoicePending.
/// Sets tickets to Refunded, order to Cancelled, and publishes OrderRevokedEvent
/// so the Lottery microservice releases sold numbers back to available.
/// </summary>
public class RevokeOrderCommandHandler : IRequestHandler<RevokeOrderCommand, Result>
{
    private const string ResourceTypeOrder = "Order";
    
    private readonly IOrderRepository _orderRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<RevokeOrderCommandHandler> _logger;

    public RevokeOrderCommandHandler(
        IOrderRepository orderRepository,
        ITicketRepository ticketRepository,
        IEventBus eventBus,
        ILogger<RevokeOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _ticketRepository = ticketRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result> Handle(RevokeOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByGuidWithTrackingAsync(request.OrderId);

        if (order is null)
            return Result.Fail(new NotFoundError("Order not found"));

        // Only revoke orders that were optimistically completed
        if (order.Status != OrderStatus.Completed)
        {
            _logger.LogWarning(
                "RevokeOrder: Order {OrderId} is in status {Status}, not Completed. Cannot revoke.",
                request.OrderId, order.Status);
            return Result.Fail(new BadRequestError(
                $"Order cannot be revoked. Current status: {order.Status}"));
        }

        try
        {
            // 1. Revoke all tickets (set to Refunded)
            var tickets = (await _ticketRepository.GetByOrderIdAsync(order.Id)).ToList();
            
            foreach (var ticket in tickets)
            {
                ticket.Status = TicketStatus.Refunded;
                await _ticketRepository.UpdateAsync(ticket);
            }

            // 2. Cancel the order
            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);

            // 3. Collect lottery number IDs for the event
            var lotteryNumberIds = order.OrderDetails
                .Where(od => od.LotteryNumberId.HasValue)
                .Select(od => od.LotteryNumberId!.Value)
                .ToList();

            // 4. Publish OrderRevokedEvent so Lottery releases Sold numbers
            await _eventBus.Publish(new OrderRevokedEvent
            {
                OrderId = order.OrderGuid,
                LotteryId = order.LotteryId,
                UserId = order.UserId,
                LotteryNumberIds = lotteryNumberIds,
                Reason = request.Reason
            });

            // 5. Audit log
            await _eventBus.Publish(new AuditLogEvent
            {
                EventType = 309, // OrderRevoked
                Source = 4,      // Order
                Action = "OrderRevoked",
                Status = 1,      // Success
                Description = $"Order {order.OrderGuid} revoked. {tickets.Count} tickets refunded. Reason: {request.Reason}",
                ResourceType = ResourceTypeOrder,
                ResourceId = order.OrderGuid.ToString(),
                Metadata = JsonSerializer.Serialize(new
                {
                    OrderId = order.OrderGuid,
                    order.InvoiceId,
                    order.UserId,
                    TicketsRevoked = tickets.Count,
                    LotteryNumbersReleased = lotteryNumberIds.Count,
                    Reason = request.Reason
                })
            });

            _logger.LogWarning(
                "Order {OrderId} REVOKED. {TicketCount} tickets set to Refunded. " +
                "{NumberCount} numbers will be released. Reason: {Reason}",
                order.OrderGuid, tickets.Count, lotteryNumberIds.Count, request.Reason);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke order {OrderId}", request.OrderId);
            return Result.Fail(new InternalServerError("Failed to revoke order"));
        }
    }
}

