using System.Text.Json;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.DTOs.CoinPayments;
using CryptoJackpot.Order.Domain.Constants;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Commands;

public class RegisterWebhookCommandHandler
    : IRequestHandler<RegisterWebhookCommand, Result<RegisterWebhookResult>>
{
    private readonly ICoinPaymentProvider _coinPaymentProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterWebhookCommandHandler> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Default notifications to subscribe to if none are specified.
    /// </summary>
    private static readonly List<string> DefaultNotifications = new()
    {
        CoinPaymentsWebhookEvents.InvoiceCreated,
        CoinPaymentsWebhookEvents.InvoicePending,
        CoinPaymentsWebhookEvents.InvoicePaid,
        CoinPaymentsWebhookEvents.InvoiceCompleted,
        CoinPaymentsWebhookEvents.InvoiceCancelled,
        CoinPaymentsWebhookEvents.InvoiceTimedOut,
        CoinPaymentsWebhookEvents.InvoicePaymentCreated,
        CoinPaymentsWebhookEvents.InvoicePaymentTimedOut
    };

    public RegisterWebhookCommandHandler(
        ICoinPaymentProvider coinPaymentProvider,
        IConfiguration configuration,
        ILogger<RegisterWebhookCommandHandler> logger)
    {
        _coinPaymentProvider = coinPaymentProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<RegisterWebhookResult>> Handle(
        RegisterWebhookCommand request,
        CancellationToken cancellationToken)
    {
        // Resolve the webhook URL: from the command or from configuration
        var notificationsUrl = request.NotificationsUrl
                               ?? _configuration[CoinPaymentsConfigKeys.WebhookNotificationsUrl];

        if (string.IsNullOrWhiteSpace(notificationsUrl))
        {
            return Result.Fail<RegisterWebhookResult>(
                new BadRequestError(
                    "WebhookNotificationsUrl is required. Provide it in the request or configure CoinPayments:WebhookNotificationsUrl"));
        }

        // Resolve notifications: from the command or use defaults
        var notifications = request.Notifications is { Count: > 0 }
            ? request.Notifications
            : DefaultNotifications;

        _logger.LogInformation(
            "Registering CoinPayments webhook. URL: {Url}, Events: [{Events}]",
            notificationsUrl, string.Join(", ", notifications));

        try
        {
            var response = await _coinPaymentProvider.RegisterWebhookAsync(
                notificationsUrl, notifications, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "CoinPayments webhook registration failed: {StatusCode} - {Content}",
                    response.StatusCode, response.Content);
                return Result.Fail<RegisterWebhookResult>(
                    new ExternalServiceError("CoinPayments",
                        $"Webhook registration failed: {response.StatusCode} - {response.Content}"));
            }

            var result = response.Deserialize<RegisterWebhookResult>(JsonOptions);

            if (result is null)
            {
                _logger.LogWarning(
                    "CoinPayments returned success but no webhook data. Content: {Content}",
                    response.Content);

                // Still consider it a success — the webhook was registered
                result = new RegisterWebhookResult
                {
                    NotificationsUrl = notificationsUrl,
                    Notifications = notifications
                };
            }

            _logger.LogInformation(
                "CoinPayments webhook registered successfully. WebhookId: {WebhookId}, URL: {Url}",
                result.Id, result.NotificationsUrl);

            return Result.Ok(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error registering CoinPayments webhook");
            return Result.Fail<RegisterWebhookResult>(
                new ExternalServiceError("CoinPayments", $"Unexpected error: {ex.Message}"));
        }
    }
}

