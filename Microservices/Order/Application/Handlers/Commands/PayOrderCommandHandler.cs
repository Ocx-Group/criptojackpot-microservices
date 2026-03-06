using System.Text.Json;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Application.DTOs.CoinPayments;
using CryptoJackpot.Order.Domain.Constants;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, Result<PayOrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICoinPaymentProvider _coinPaymentProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PayOrderCommandHandler> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PayOrderCommandHandler(
        IOrderRepository orderRepository,
        ICoinPaymentProvider coinPaymentProvider,
        IConfiguration configuration,
        ILogger<PayOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _coinPaymentProvider = coinPaymentProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<PayOrderResponse>> Handle(
        PayOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByGuidAsync(request.OrderId);

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
                Name = $"Lottery Ticket #{detail.Number}-{detail.Series}",
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
                return Result.Fail<PayOrderResponse>(
                    new ExternalServiceError("CoinPayments",
                        $"Payment service returned {response.StatusCode}"));
            }

            var apiResponse = response.Deserialize<CoinPaymentsApiResponse<CreateInvoiceResult>>(JsonOptions);
            var invoice = apiResponse?.FirstResult;

            if (invoice is null)
            {
                _logger.LogError(
                    "CoinPayments returned success but no invoice data for order {OrderId}. Content: {Content}",
                    request.OrderId, response.Content);
                return Result.Fail<PayOrderResponse>(
                    new ExternalServiceError("CoinPayments", "No invoice returned in response"));
            }

            var secondsRemaining = (int)(order.ExpiresAt - DateTime.UtcNow).TotalSeconds;

            _logger.LogInformation(
                "CoinPayments invoice {InvoiceId} created for order {OrderId}. CheckoutUrl: {CheckoutUrl}",
                invoice.InvoiceId, order.OrderGuid, invoice.CheckoutUrl);

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
