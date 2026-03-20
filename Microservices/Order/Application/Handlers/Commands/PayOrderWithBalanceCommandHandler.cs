using System.Text.Json;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Audit;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

/// <summary>
/// Handles payment with internal wallet balance.
/// Flow: validate order → debit wallet via gRPC → complete order (create tickets) via MediatR.
/// </summary>
public class PayOrderWithBalanceCommandHandler : IRequestHandler<PayOrderWithBalanceCommand, Result<TicketDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IWalletDebitGrpcClient _walletDebitClient;
    private readonly IMediator _mediator;
    private readonly IEventBus _eventBus;
    private readonly ILogger<PayOrderWithBalanceCommandHandler> _logger;

    public PayOrderWithBalanceCommandHandler(
        IOrderRepository orderRepository,
        IWalletDebitGrpcClient walletDebitClient,
        IMediator mediator,
        IEventBus eventBus,
        ILogger<PayOrderWithBalanceCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _walletDebitClient = walletDebitClient;
        _mediator = mediator;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<TicketDto>> Handle(
        PayOrderWithBalanceCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByGuidWithTrackingAsync(request.OrderId);

        if (order is null)
            return Result.Fail<TicketDto>(new NotFoundError("Order not found"));

        if (order.UserId != request.UserId)
            return Result.Fail<TicketDto>(
                new ForbiddenError("You don't have permission to pay for this order"));

        if (order.Status != OrderStatus.Pending)
            return Result.Fail<TicketDto>(
                new BadRequestError($"Order cannot be paid. Current status: {order.Status}"));

        if (order.IsExpired)
            return Result.Fail<TicketDto>(
                new BadRequestError("Order has expired. Please create a new order."));

        try
        {
            // Step 1: Debit internal wallet balance via gRPC
            var debitResult = await _walletDebitClient.DebitBalanceAsync(
                userGuid: request.UserGuid,
                amount: order.TotalAmount,
                orderId: order.OrderGuid,
                description: $"Ticket purchase - Order {order.OrderGuid}",
                cancellationToken: cancellationToken);

            if (!debitResult.Success)
            {
                _logger.LogWarning(
                    "Balance debit rejected for order {OrderId}: {Error}",
                    request.OrderId, debitResult.ErrorMessage);

                await _eventBus.Publish(new AuditLogEvent
                {
                    EventType = 505,
                    Source = 4,
                    Action = "PayOrderWithBalance",
                    Status = 2,
                    Description = $"Balance payment failed for order {request.OrderId}: {debitResult.ErrorMessage}",
                    ResourceType = "Order",
                    ResourceId = request.OrderId.ToString(),
                    ErrorMessage = debitResult.ErrorMessage,
                    Metadata = JsonSerializer.Serialize(new
                    {
                        request.OrderId,
                        request.UserId,
                        request.UserGuid,
                        Amount = order.TotalAmount
                    })
                });

                return Result.Fail<TicketDto>(
                    new BadRequestError(debitResult.ErrorMessage ?? "Insufficient balance"));
            }

            // Step 2: Complete the order — creates tickets and publishes events
            var completeResult = await _mediator.Send(new CompleteOrderCommand
            {
                OrderId = request.OrderId,
                UserId = request.UserId,
                TransactionId = debitResult.TransactionGuid ?? Guid.NewGuid().ToString()
            }, cancellationToken);

            if (completeResult.IsFailed)
            {
                _logger.LogError(
                    "Order completion failed after balance debit for order {OrderId}. " +
                    "Balance was debited (tx: {TxGuid}). Manual reconciliation may be needed.",
                    request.OrderId, debitResult.TransactionGuid);

                return completeResult;
            }

            // Publish audit event for successful balance payment
            await _eventBus.Publish(new AuditLogEvent
            {
                EventType = 505,
                Source = 4,
                Action = "PayOrderWithBalance",
                Status = 1,
                Description = $"Order {order.OrderGuid} paid with internal balance. Amount: {order.TotalAmount}",
                ResourceType = "Order",
                ResourceId = order.OrderGuid.ToString(),
                Metadata = JsonSerializer.Serialize(new
                {
                    OrderId = order.OrderGuid,
                    request.UserId,
                    request.UserGuid,
                    Amount = order.TotalAmount,
                    debitResult.TransactionGuid,
                    debitResult.BalanceAfter
                })
            });

            _logger.LogInformation(
                "Order {OrderId} paid with internal balance. Amount: {Amount}, TxGuid: {TxGuid}",
                order.OrderGuid, order.TotalAmount, debitResult.TransactionGuid);

            return completeResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process balance payment for order {OrderId}", request.OrderId);
            return Result.Fail<TicketDto>(
                new InternalServerError("Failed to process balance payment"));
        }
    }
}
