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

    /// <summary>
    /// Retrieves a list of the merchant's wallets.
    /// GET /merchant/wallets
    /// </summary>
    Task<RestResponse> GetMerchantWalletsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a spend request (withdrawal or conversion) from a merchant wallet.
    /// Returns a transaction preview including fees. Confirm with the confirm spend request endpoint.
    /// POST /merchant/wallets/{id}/spend/request
    /// </summary>
    /// <param name="walletId">Path parameter — ID of the wallet to spend from.</param>
    /// <param name="params">Body payload with destination, amount and optional overrides.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<RestResponse> CreateSpendRequestAsync(
        string walletId,
        SpendRequestParams @params,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms a pending spend request, publishing the transaction to the blockchain.
    /// POST /merchant/wallets/{id}/spend/confirmation
    /// </summary>
    /// <param name="walletId">Path parameter — ID of the wallet to spend from.</param>
    /// <param name="spendRequestId">ID returned by <see cref="CreateSpendRequestAsync"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<RestResponse> ConfirmSpendRequestAsync(
        string walletId,
        string spendRequestId,
        CancellationToken cancellationToken = default);
}

