using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Order.Application.DTOs.CoinPayments;
using CryptoJackpot.Order.Domain.Interfaces;
using CryptoJackpot.Order.Domain.Models;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Consumers;

/// <summary>
/// Consumes WithdrawalApprovedEvent from Wallet microservice to process
/// the CoinPayments spend (create + confirm) and publish result events.
/// </summary>
public class WithdrawalApprovedConsumer : IConsumer<WithdrawalApprovedEvent>
{
    private readonly ICoinPaymentProvider _coinPaymentProvider;
    private readonly IEventBus _eventBus;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WithdrawalApprovedConsumer> _logger;

    public WithdrawalApprovedConsumer(
        ICoinPaymentProvider coinPaymentProvider,
        IEventBus eventBus,
        IConfiguration configuration,
        ILogger<WithdrawalApprovedConsumer> logger)
    {
        _coinPaymentProvider = coinPaymentProvider;
        _eventBus = eventBus;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WithdrawalApprovedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing withdrawal spend for {RequestGuid}: ${Amount} to {Address} ({Currency})",
            message.RequestGuid, message.Amount, message.WalletAddress, message.CurrencySymbol);

        try
        {
            // 1. Discover the merchant wallet for this currency
            var (walletId, currencyId) = await ResolveMerchantWalletAsync(
                message.CurrencySymbol, context.CancellationToken);

            if (walletId is null || currencyId is null)
            {
                await PublishFailure(message,
                    $"No merchant wallet found for currency {message.CurrencySymbol}.");
                return;
            }

            // 2. Create spend request
            var spendParams = new SpendRequestParams
            {
                ToAddress = message.WalletAddress,
                ToCurrency = currencyId,
                Amount = message.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                AmountCurrency = _configuration["CoinPayments:UsdCurrencyId"] ?? "5057",
                Memo = $"Withdrawal {message.RequestGuid}",
            };

            var spendResponse = await _coinPaymentProvider.CreateSpendRequestAsync(
                walletId, spendParams, context.CancellationToken);

            if (!spendResponse.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "CoinPayments CreateSpendRequest failed for {RequestGuid}: {StatusCode} - {Content}",
                    message.RequestGuid, spendResponse.StatusCode, spendResponse.Content);
                await PublishFailure(message, $"CoinPayments spend request failed: {spendResponse.Content}");
                return;
            }

            // 3. Parse spend request result
            if (!spendResponse.TryDeserialize<CoinPaymentsApiResponse<SpendRequestResult>>(out var spendResult)
                || spendResult?.FirstResult is null)
            {
                _logger.LogError(
                    "Failed to parse CoinPayments spend response for {RequestGuid}: {Content}",
                    message.RequestGuid, spendResponse.Content);
                await PublishFailure(message, "Invalid response from CoinPayments.");
                return;
            }

            var spendRequestId = spendResult.FirstResult.SpendRequestId;

            _logger.LogInformation(
                "CoinPayments spend request created for {RequestGuid}: spendRequestId={SpendRequestId}",
                message.RequestGuid, spendRequestId);

            // 4. Confirm the spend request (publish to blockchain)
            var confirmResponse = await _coinPaymentProvider.ConfirmSpendRequestAsync(
                walletId, spendRequestId, context.CancellationToken);

            if (!confirmResponse.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "CoinPayments ConfirmSpendRequest failed for {RequestGuid}: {StatusCode} - {Content}",
                    message.RequestGuid, confirmResponse.StatusCode, confirmResponse.Content);
                await PublishFailure(message, $"CoinPayments spend confirmation failed: {confirmResponse.Content}");
                return;
            }

            // 5. Publish success event back to Wallet
            await _eventBus.Publish(new WithdrawalCompletedEvent
            {
                RequestGuid = message.RequestGuid,
                UserGuid = message.UserGuid,
                Amount = message.Amount,
                TransactionId = spendRequestId
            });

            _logger.LogInformation(
                "Withdrawal spend completed for {RequestGuid}: ${Amount} sent to {Address}",
                message.RequestGuid, message.Amount, message.WalletAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error processing withdrawal spend for {RequestGuid}",
                message.RequestGuid);
            await PublishFailure(message, $"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Resolves the CoinPayments merchant wallet ID and currency ID for the given currency symbol.
    /// If no wallet exists, creates one automatically.
    /// </summary>
    private async Task<(string? WalletId, string? CurrencyId)> ResolveMerchantWalletAsync(
        string currencySymbol, CancellationToken cancellationToken)
    {
        // First, resolve the currencyId from the symbol (needed for wallet creation)
        var currencyId = await ResolveCurrencyIdAsync(currencySymbol, cancellationToken);
        if (currencyId is null)
            return (null, null);

        var walletsResponse = await _coinPaymentProvider.GetMerchantWalletsAsync(cancellationToken);
        if (!walletsResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch merchant wallets: {StatusCode} - {Content}",
                walletsResponse.StatusCode, walletsResponse.Content);
            return (null, null);
        }

        var wallets = ParseMerchantWallets(walletsResponse);

        if (wallets is not null && wallets.Count > 0)
        {
            // Match by currencySymbol directly (the API returns it)
            var wallet = wallets.FirstOrDefault(w =>
                w.CurrencySymbol.Equals(currencySymbol, StringComparison.OrdinalIgnoreCase)
                && w.IsActive && !w.IsLocked);

            // Fallback: try without active/locked filter
            wallet ??= wallets.FirstOrDefault(w =>
                w.CurrencySymbol.Equals(currencySymbol, StringComparison.OrdinalIgnoreCase));

            if (wallet is not null)
            {
                _logger.LogInformation(
                    "Resolved existing merchant wallet {WalletId} for {Symbol} (currencyId: {CurrencyId})",
                    wallet.WalletId, currencySymbol, wallet.CurrencyId);
                return (wallet.WalletId, wallet.CurrencyId);
            }

            _logger.LogWarning(
                "No merchant wallet for {Symbol} among {Count} wallets: [{Wallets}]",
                currencySymbol, wallets.Count,
                string.Join(", ", wallets.Select(w =>
                    $"{w.CurrencySymbol}(id:{w.WalletId}, active:{w.IsActive})")));
        }
        else
        {
            _logger.LogWarning("No merchant wallets found at all. Will create one for {Symbol}.", currencySymbol);
        }

        // Auto-create the wallet
        return await CreateMerchantWalletAsync(currencyId, currencySymbol, cancellationToken);
    }

    /// <summary>
    /// Resolves the CoinPayments currency ID from a symbol (e.g. "LTCT" → "1002").
    /// </summary>
    private async Task<string?> ResolveCurrencyIdAsync(string currencySymbol, CancellationToken cancellationToken)
    {
        var currenciesResponse = await _coinPaymentProvider.GetCurrenciesAsync(cancellationToken);
        if (!currenciesResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch currencies: {StatusCode} - {Content}",
                currenciesResponse.StatusCode, currenciesResponse.Content);
            return null;
        }

        if (!currenciesResponse.TryDeserialize<CurrencyResult[]>(out var currencies) || currencies is null)
        {
            _logger.LogError("Failed to parse currencies response");
            return null;
        }

        var currency = currencies.FirstOrDefault(c =>
            c.Symbol.Equals(currencySymbol, StringComparison.OrdinalIgnoreCase));

        if (currency is null)
        {
            _logger.LogError("Currency {Symbol} not found in CoinPayments", currencySymbol);
            return null;
        }

        return currency.Id;
    }

    /// <summary>
    /// Creates a new merchant wallet for the given currency via the CoinPayments API.
    /// </summary>
    private async Task<(string? WalletId, string? CurrencyId)> CreateMerchantWalletAsync(
        string currencyId, string currencySymbol, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating merchant wallet for {Symbol} (currencyId: {CurrencyId})",
            currencySymbol, currencyId);

        var createResponse = await _coinPaymentProvider.CreateMerchantWalletAsync(
            currencyId, $"CriptoJackpot {currencySymbol}", cancellationToken);

        if (!createResponse.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Failed to create merchant wallet for {Symbol}: {StatusCode} - {Content}",
                currencySymbol, createResponse.StatusCode, createResponse.Content);
            return (null, null);
        }

        if (!createResponse.TryDeserialize<CreateMerchantWalletResult>(out var walletResult)
            || walletResult is null || string.IsNullOrEmpty(walletResult.WalletId))
        {
            _logger.LogError(
                "Failed to parse create wallet response for {Symbol}: {Content}",
                currencySymbol, createResponse.Content);
            return (null, null);
        }

        _logger.LogInformation(
            "Created merchant wallet {WalletId} for {Symbol} (address: {Address})",
            walletResult.WalletId, currencySymbol, walletResult.Address);

        return (walletResult.WalletId, currencyId);
    }

    /// <summary>
    /// Parses the merchant wallets API response (handles both array and items-wrapper formats).
    /// </summary>
    private static List<MerchantWalletResult>? ParseMerchantWallets(RestResponse response)
    {
        // Try as paginated { items: [...] }
        if (response.TryDeserialize<CoinPaymentsApiResponse<MerchantWalletResult>>(out var wrapped)
            && wrapped?.Items is not null && wrapped.Items.Count > 0)
        {
            return wrapped.Items;
        }

        // Try as raw array [...]
        if (response.TryDeserialize<MerchantWalletResult[]>(out var array)
            && array is not null && array.Length > 0)
        {
            return array.ToList();
        }

        return null;
    }

    private async Task PublishFailure(WithdrawalApprovedEvent message, string reason)
    {
        _logger.LogWarning(
            "Publishing WithdrawalFailedEvent for {RequestGuid}: {Reason}",
            message.RequestGuid, reason);

        await _eventBus.Publish(new WithdrawalFailedEvent
        {
            RequestGuid = message.RequestGuid,
            UserGuid = message.UserGuid,
            Amount = message.Amount,
            Reason = reason
        });
    }
}
