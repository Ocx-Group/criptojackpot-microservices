using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Order.Domain.Models;

namespace CryptoJackpot.Order.Domain.Interfaces;

public interface ICoinPaymentProvider
{
    Task<RestResponse> CreateInvoiceAsync(
        decimal amount,
        string currency,
        List<InvoiceLineItem> items,
        string? description = null,
        string? notificationsUrl = null,
        CancellationToken cancellationToken = default);

    Task<RestResponse> GetInvoiceAsync(
        string invoiceId,
        CancellationToken cancellationToken = default);

    Task<RestResponse> GetCurrenciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a webhook on CoinPayments for the given client integration.
    /// POST /merchant/clients/{clientId}/webhooks
    /// </summary>
    Task<RestResponse> RegisterWebhookAsync(
        string notificationsUrl,
        List<string> notifications,
        CancellationToken cancellationToken = default);
}

