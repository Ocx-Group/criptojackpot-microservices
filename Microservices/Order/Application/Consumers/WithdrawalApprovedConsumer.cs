using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
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
    /// Calls the merchant wallets API and the currencies API to find the match.
    /// </summary>
    private async Task<(string? WalletId, string? CurrencyId)> ResolveMerchantWalletAsync(
        string currencySymbol, CancellationToken cancellationToken)
    {
        // Get currencies to map symbol → currency ID
        var currenciesResponse = await _coinPaymentProvider.GetCurrenciesAsync(cancellationToken);
        if (!currenciesResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch CoinPayments currencies: {Content}", currenciesResponse.Content);
            return (null, null);
        }

        if (!currenciesResponse.TryDeserialize<CurrencyResult[]>(out var currencies) || currencies is null)
        {
            _logger.LogError("Failed to parse CoinPayments currencies response");
            return (null, null);
        }

        var currency = currencies.FirstOrDefault(c =>
            c.Symbol.Equals(currencySymbol, StringComparison.OrdinalIgnoreCase));

        if (currency is null)
        {
            _logger.LogError("Currency {Symbol} not found in CoinPayments", currencySymbol);
            return (null, null);
        }

        var currencyId = currency.Id;

        // Get merchant wallets and find the one matching this currency
        var walletsResponse = await _coinPaymentProvider.GetMerchantWalletsAsync(cancellationToken);
        if (!walletsResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch merchant wallets: {Content}", walletsResponse.Content);
            return (null, null);
        }

        if (!walletsResponse.TryDeserialize<CoinPaymentsApiResponse<MerchantWalletResult>>(out var walletsResult)
            || walletsResult?.Items is null)
        {
            _logger.LogError("Failed to parse merchant wallets response: {Content}", walletsResponse.Content);
            return (null, null);
        }

        var wallet = walletsResult.Items.FirstOrDefault(w => w.CurrencyId == currencyId);

        if (wallet is null)
        {
            _logger.LogWarning(
                "No merchant wallet found for currency {Symbol} (ID: {CurrencyId}). Available wallets: {Wallets}",
                currencySymbol, currencyId,
                string.Join(", ", walletsResult.Items.Select(w => $"{w.CurrencyId}:{w.Id}")));
            return (null, null);
        }

        _logger.LogInformation(
            "Resolved merchant wallet {WalletId} for currency {Symbol} (ID: {CurrencyId})",
            wallet.Id, currencySymbol, currencyId);

        return (wallet.Id, currencyId);
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
