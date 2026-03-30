using AutoMapper;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using CryptoJackpot.Order.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, Result<TicketDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IEventBus _eventBus;
    private readonly IMapper _mapper;
    private readonly ILogger<CompleteOrderCommandHandler> _logger;

    public CompleteOrderCommandHandler(
        IOrderRepository orderRepository,
        ITicketRepository ticketRepository,
        IEventBus eventBus,
        IMapper mapper,
        ILogger<CompleteOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _ticketRepository = ticketRepository;
        _eventBus = eventBus;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TicketDto>> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByGuidWithTrackingAsync(request.OrderId);

        if (order is null)
            return Result.Fail<TicketDto>(new NotFoundError("Order not found"));

        // Verify the order belongs to the user
        if (order.UserId != request.UserId)
            return Result.Fail<TicketDto>(new ForbiddenError("You don't have permission to complete this order"));

        // Verify the order is in Pending status
        if (order.Status != OrderStatus.Pending)
            return Result.Fail<TicketDto>(new BadRequestError($"Order cannot be completed. Current status: {order.Status}"));

        // Get lottery number IDs for events
        var lotteryNumberIds = order.OrderDetails
            .Where(od => od.LotteryNumberId.HasValue)
            .Select(od => od.LotteryNumberId!.Value)
            .ToList();

        // Verify the order hasn't expired
        if (order.IsExpired)
        {
            order.Status = OrderStatus.Expired;
            await _orderRepository.UpdateAsync(order);

            // Publish expired event to release numbers
            await _eventBus.Publish(new OrderExpiredEvent
            {
                OrderId = order.OrderGuid,
                LotteryId = order.LotteryId,
                LotteryNumberIds = lotteryNumberIds
            });
            
            _logger.LogWarning("Order {OrderId} has expired", request.OrderId);
            return Result.Fail<TicketDto>(new BadRequestError("Order has expired. Please create a new order."));
        }

        try
        {
            var now = DateTime.UtcNow;
            Ticket? firstTicket = null;

            // Create a ticket for each order detail
            foreach (var detail in order.OrderDetails)
            {
                var ticket = new Ticket
                {
                    TicketGuid = Guid.NewGuid(),
                    OrderDetailId = detail.Id,
                    LotteryId = order.LotteryId,
                    UserId = detail.IsGift && detail.GiftRecipientId.HasValue 
                        ? detail.GiftRecipientId.Value 
                        : order.UserId,
                    PurchaseAmount = detail.Subtotal,
                    PurchaseDate = now,
                    Status = TicketStatus.Active,
                    TransactionId = request.TransactionId,
                    Number = detail.Number,
                    Series = detail.Series,
                    LotteryNumberId = detail.LotteryNumberId,
                    IsGift = detail.IsGift,
                    GiftSenderId = detail.IsGift ? order.UserId : null
                };

                var createdTicket = await _ticketRepository.CreateAsync(ticket);
                detail.TicketId = createdTicket.TicketGuid;
                
                firstTicket ??= createdTicket;
            }

            // Update order to completed
            order.Status = OrderStatus.Completed;
            await _orderRepository.UpdateAsync(order);

            // Publish event to mark numbers as sold permanently
            await _eventBus.Publish(new OrderCompletedEvent
            {
                OrderId = order.OrderGuid,
                TicketId = firstTicket!.TicketGuid,
                LotteryId = order.LotteryId,
                UserId = order.UserId,
                BuyerUserGuid = order.UserGuid,
                LotteryNumberIds = lotteryNumberIds,
                TransactionId = request.TransactionId,
                // Notification data
                UserEmail = order.UserEmail,
                UserName = order.UserName,
                LotteryTitle = order.LotteryTitle,
                LotteryNo = order.LotteryNo,
                TotalAmount = order.TotalAmount,
                Tickets = order.OrderDetails.Select(d => new PurchasedTicketItem
                {
                    Number = d.Number,
                    Series = d.Series,
                    Amount = d.Subtotal,
                    IsGift = d.IsGift,
                    GiftRecipientId = d.GiftRecipientId
                }).ToList(),
                LotteryType = order.LotteryType
            });

            _logger.LogInformation(
                "Order {OrderId} completed. {TicketCount} tickets created for user {UserId}. Transaction: {TransactionId}.",
                order.OrderGuid, order.OrderDetails.Count, request.UserId, request.TransactionId);

            return Result.Ok(_mapper.Map<TicketDto>(firstTicket));
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("23505") == true
                                            || ex.InnerException?.Message.Contains("duplicate key") == true)
        {
            // Race condition: CoinPayments webhook retry hit another pod while this pod was
            // already completing the same order. The first pod created the tickets and set
            // the order to Completed. Return the existing ticket (idempotent).
            _logger.LogWarning(
                "Order {OrderId} concurrent completion detected (duplicate key). Returning existing ticket.",
                request.OrderId);

            var existingTickets = await _ticketRepository.GetByOrderIdAsync(order.Id);
            var existingTicket = existingTickets.FirstOrDefault();

            if (existingTicket is not null)
                return Result.Ok(_mapper.Map<TicketDto>(existingTicket));

            _logger.LogError(ex, "Order {OrderId} duplicate key but no existing ticket found", request.OrderId);
            return Result.Fail<TicketDto>(new InternalServerError("Failed to complete order"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete order {OrderId}", request.OrderId);
            return Result.Fail<TicketDto>(new InternalServerError("Failed to complete order"));
        }
    }
}

