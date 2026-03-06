using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Domain.Constants;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

public class ProcessWebhookCommandHandler : IRequestHandler<ProcessWebhookCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessWebhookCommandHandler> _logger;

    public ProcessWebhookCommandHandler(
        IOrderRepository orderRepository,
        IMediator mediator,
        ILogger<ProcessWebhookCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result> Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing CoinPayments webhook. EventType: {EventType}, InvoiceId: {InvoiceId}",
            request.EventType, request.InvoiceId);

        // Resolve the CoinPayments invoice ID from the payload
        var coinPaymentsInvoiceId = request.Payload.Invoice?.Id ?? request.InvoiceId;

        if (string.IsNullOrWhiteSpace(coinPaymentsInvoiceId))
        {
            _logger.LogWarning("Webhook received without a valid invoice ID");
            return Result.Fail(new BadRequestError("Missing invoice ID in webhook payload"));
        }

        // Look up the order by the CoinPayments invoice ID
        var order = await _orderRepository.GetByInvoiceIdWithTrackingAsync(coinPaymentsInvoiceId);

        if (order is null)
        {
            _logger.LogWarning(
                "No order found for CoinPayments InvoiceId: {InvoiceId}. Event: {EventType}",
                coinPaymentsInvoiceId, request.EventType);
            return Result.Fail(new NotFoundError($"No order found for invoice {coinPaymentsInvoiceId}"));
        }

        var eventType = request.EventType;

        // Handle event types (case-insensitive as recommended by CoinPayments)
        if (eventType.Equals(CoinPaymentsWebhookEvents.InvoicePaid, StringComparison.OrdinalIgnoreCase) ||
            eventType.Equals(CoinPaymentsWebhookEvents.InvoiceCompleted, StringComparison.OrdinalIgnoreCase))
        {
            return await HandleInvoicePaid(order, request, cancellationToken);
        }

        if (eventType.Equals(CoinPaymentsWebhookEvents.InvoiceCancelled, StringComparison.OrdinalIgnoreCase))
        {
            return await HandleInvoiceCancelled(order, cancellationToken);
        }

        if (eventType.Equals(CoinPaymentsWebhookEvents.InvoiceTimedOut, StringComparison.OrdinalIgnoreCase))
        {
            return await HandleInvoiceTimedOut(order, cancellationToken);
        }

        // Informational events: InvoiceCreated, InvoicePending, InvoicePaymentCreated, InvoicePaymentTimedOut
        _logger.LogInformation(
            "Informational webhook received. EventType: {EventType}, OrderId: {OrderId}, InvoiceId: {InvoiceId}",
            request.EventType, order.OrderGuid, coinPaymentsInvoiceId);

        return Result.Ok();
    }

    private async Task<Result> HandleInvoicePaid(
        Domain.Models.Order order,
        ProcessWebhookCommand request,
        CancellationToken cancellationToken)
    {
        // Idempotency: if already completed, skip
        if (order.Status == OrderStatus.Completed)
        {
            _logger.LogInformation(
                "Order {OrderId} already completed. Skipping duplicate invoicePaid/invoiceCompleted webhook",
                order.OrderGuid);
            return Result.Ok();
        }

        // Cannot complete if not in Pending status
        if (order.Status != OrderStatus.Pending)
        {
            _logger.LogWarning(
                "Order {OrderId} is in status {Status}. Cannot complete from webhook",
                order.OrderGuid, order.Status);
            return Result.Fail(new BadRequestError(
                $"Order cannot be completed. Current status: {order.Status}"));
        }

        // Extract transaction ID from payment details
        var transactionId = request.Payload.Invoice?.Payments?.FirstOrDefault()?.TransactionId
                            ?? request.Payload.Invoice?.Id
                            ?? request.InvoiceId;

        _logger.LogInformation(
            "Completing order {OrderId} via webhook. TransactionId: {TransactionId}",
            order.OrderGuid, transactionId);

        var completeResult = await _mediator.Send(new CompleteOrderCommand
        {
            OrderId = order.OrderGuid,
            UserId = order.UserId,
            TransactionId = transactionId
        }, cancellationToken);

        if (completeResult.IsFailed)
        {
            _logger.LogError(
                "Failed to complete order {OrderId} from webhook: {Error}",
                order.OrderGuid, completeResult.Errors.FirstOrDefault()?.Message);
            return Result.Fail(completeResult.Errors);
        }

        _logger.LogInformation(
            "Order {OrderId} successfully completed via CoinPayments webhook",
            order.OrderGuid);

        return Result.Ok();
    }

    private async Task<Result> HandleInvoiceCancelled(
        Domain.Models.Order order,
        CancellationToken cancellationToken)
    {
        // Idempotency: if already cancelled, skip
        if (order.Status == OrderStatus.Cancelled)
        {
            _logger.LogInformation(
                "Order {OrderId} already cancelled. Skipping duplicate invoiceCancelled webhook",
                order.OrderGuid);
            return Result.Ok();
        }

        // Cannot cancel if not in Pending status
        if (order.Status != OrderStatus.Pending)
        {
            _logger.LogWarning(
                "Order {OrderId} is in status {Status}. Cannot cancel from webhook",
                order.OrderGuid, order.Status);
            return Result.Ok(); // Not an error, just nothing to do
        }

        _logger.LogInformation(
            "Cancelling order {OrderId} via webhook (invoiceCancelled)",
            order.OrderGuid);

        var cancelResult = await _mediator.Send(new CancelOrderCommand
        {
            OrderId = order.OrderGuid,
            UserId = order.UserId,
            Reason = "Cancelled via CoinPayments webhook"
        }, cancellationToken);

        if (cancelResult.IsFailed)
        {
            _logger.LogError(
                "Failed to cancel order {OrderId} from webhook: {Error}",
                order.OrderGuid, cancelResult.Errors.FirstOrDefault()?.Message);
            return Result.Fail(cancelResult.Errors);
        }

        _logger.LogInformation(
            "Order {OrderId} successfully cancelled via CoinPayments webhook",
            order.OrderGuid);

        return Result.Ok();
    }

    private async Task<Result> HandleInvoiceTimedOut(
        Domain.Models.Order order,
        CancellationToken cancellationToken)
    {
        // Idempotency: if already expired or cancelled, skip
        if (order.Status is OrderStatus.Expired or OrderStatus.Cancelled)
        {
            _logger.LogInformation(
                "Order {OrderId} already {Status}. Skipping duplicate invoiceTimedOut webhook",
                order.OrderGuid, order.Status);
            return Result.Ok();
        }

        // Cannot expire if already completed
        if (order.Status == OrderStatus.Completed)
        {
            _logger.LogWarning(
                "Order {OrderId} is already completed. Ignoring invoiceTimedOut webhook",
                order.OrderGuid);
            return Result.Ok();
        }

        _logger.LogInformation(
            "Cancelling order {OrderId} via webhook (invoiceTimedOut)",
            order.OrderGuid);

        var cancelResult = await _mediator.Send(new CancelOrderCommand
        {
            OrderId = order.OrderGuid,
            UserId = order.UserId,
            Reason = "Invoice timed out via CoinPayments webhook"
        }, cancellationToken);

        if (cancelResult.IsFailed)
        {
            _logger.LogError(
                "Failed to expire order {OrderId} from webhook: {Error}",
                order.OrderGuid, cancelResult.Errors.FirstOrDefault()?.Message);
            return Result.Fail(cancelResult.Errors);
        }

        _logger.LogInformation(
            "Order {OrderId} successfully expired via CoinPayments webhook (invoiceTimedOut)",
            order.OrderGuid);

        return Result.Ok();
    }
}

