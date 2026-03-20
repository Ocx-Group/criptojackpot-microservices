using System.Text.Json;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Audit;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.Converters;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Application.DTOs.CoinPayments;
using CryptoJackpot.Order.Domain.Constants;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using CryptoJackpot.Order.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, Result<PayOrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICoinPaymentProvider _coinPaymentProvider;
    private readonly IEventBus _eventBus;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PayOrderCommandHandler> _logger;


    public PayOrderCommandHandler(
        IOrderRepository orderRepository,
        ICoinPaymentProvider coinPaymentProvider,
        IEventBus eventBus,
        IConfiguration configuration,
        ILogger<PayOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _coinPaymentProvider = coinPaymentProvider;
        _eventBus = eventBus;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<PayOrderResponse>> Handle(
        PayOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByGuidWithTrackingAsync(request.OrderId);

        if (order is null)
            return Result.Fail<PayOrderResponse>(new NotFoundError("Order not found"));

        if (order.UserId != request.UserId)
            return Result.Fail<PayOrderResponse>(
                new ForbiddenError("You don't have permission to pay for this order"));

        if (order.Status != OrderStatus.Pending)
            return Result.Fail<PayOrderResponse>(
                new BadRequestError($"Order cannot be paid. Current status: {order.Status}"));

        if (order.IsExpired)
            return Result.Fail<PayOrderResponse>(
                new BadRequestError("Order has expired. Please create a new order."));

        try
        {
            var invoiceItems = order.OrderDetails.Select(detail => new InvoiceLineItem
            {
                Name = $"Lottery Ticket #{detail.Number}-{detail.Series.ToString().PadLeft(2, '0')}",
                Quantity = detail.Quantity,
                Amount = detail.UnitPrice
            }).ToList();

            var currency = _configuration[CoinPaymentsConfigKeys.InvoiceCurrency]
                           ?? CoinPaymentsDefaults.DefaultInvoiceCurrency;
            var webhookUrl = _configuration[CoinPaymentsConfigKeys.WebhookNotificationsUrl];

            var description = $"CriptoJackpot Order {order.OrderGuid}";

            var response = await _coinPaymentProvider.CreateInvoiceAsync(
                amount: order.TotalAmount,
                currency: currency,
                items: invoiceItems,
                description: description,
                notificationsUrl: webhookUrl,
                cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "CoinPayments API error for order {OrderId}: {StatusCode} - {Content}",
                    request.OrderId, response.StatusCode, response.Content);

                await _eventBus.Publish(new AuditLogEvent
                {
                    EventType = 504, // OrderPaymentInitiated
                    Source = 4,      // Order
                    Action = "PayOrder",
                    Status = 2,      // Failed
                    Description = $"Payment initiation failed for order {request.OrderId}. CoinPayments returned {response.StatusCode}",
                    ResourceType = "Order",
                    ResourceId = request.OrderId.ToString(),
                    ErrorMessage = $"CoinPayments API error: {response.StatusCode} - {response.Content}",
                    Metadata = JsonSerializer.Serialize(new
                    {
                        request.OrderId,
                        request.UserId,
                        StatusCode = (int)response.StatusCode
                    })
                });

                return Result.Fail<PayOrderResponse>(
                    new ExternalServiceError("CoinPayments",
                        $"Payment service returned {response.StatusCode}"));
            }

            var apiResponse = response.Deserialize<CoinPaymentsApiResponse<CreateInvoiceResult>>(JsonDefaults.ApiResponse);
            var invoice = apiResponse?.FirstResult;

            if (invoice is null)
            {
                _logger.LogError(
                    "CoinPayments returned success but no invoice data for order {OrderId}. Content: {Content}",
                    request.OrderId, response.Content);
                return Result.Fail<PayOrderResponse>(
                    new ExternalServiceError("CoinPayments", "No invoice returned in response"));
            }

            // Persist the CoinPayments invoice ID on the order for webhook correlation
            order.InvoiceId = invoice.InvoiceId;
            await _orderRepository.UpdateAsync(order);

            var secondsRemaining = (int)(order.ExpiresAt - DateTime.UtcNow).TotalSeconds;

            _logger.LogInformation(
                "CoinPayments invoice {InvoiceId} created for order {OrderId}. CheckoutUrl: {CheckoutUrl}",
                invoice.InvoiceId, order.OrderGuid, invoice.CheckoutUrl);

            // Publish audit event for payment initiation
            await _eventBus.Publish(new AuditLogEvent
            {
                EventType = 504, // OrderPaymentInitiated
                Source = 4,      // Order
                Action = "PayOrder",
                Status = 1,      // Success
                Description = $"Payment initiated for order {order.OrderGuid}. CoinPayments invoice {invoice.InvoiceId} created. Amount: {order.TotalAmount}",
                ResourceType = "Order",
                ResourceId = order.OrderGuid.ToString(),
                Metadata = JsonSerializer.Serialize(new
                {
                    OrderId = order.OrderGuid,
                    invoice.InvoiceId,
                    Amount = order.TotalAmount,
                    Currency = currency,
                    invoice.CheckoutUrl,
                    request.UserId,
                    ItemCount = order.TotalItems
                })
            });

            return Result.Ok(new PayOrderResponse
            {
                OrderId = order.OrderGuid,
                InvoiceId = invoice.InvoiceId,
                CheckoutUrl = invoice.CheckoutUrl,
                StatusUrl = invoice.StatusUrl,
                QrCodeUrl = invoice.QrCodeUrl,
                TotalAmount = order.TotalAmount,
                SecondsRemaining = Math.Max(secondsRemaining, 0)
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment for order {OrderId}", request.OrderId);
            return Result.Fail<PayOrderResponse>(
                new ExternalServiceError("CoinPayments", $"Unexpected error: {ex.Message}"));
        }
    }
}
