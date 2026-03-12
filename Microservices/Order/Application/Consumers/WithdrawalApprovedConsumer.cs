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
            var merchantWalletId = _configuration[$"CoinPayments:MerchantWallets:{message.CurrencySymbol}"];
            if (string.IsNullOrWhiteSpace(merchantWalletId))
            {
                await PublishFailure(message, $"No CoinPayments merchant wallet configured for {message.CurrencySymbol}.");
                return;
            }

            var currencyId = _configuration[$"CoinPayments:CurrencyIds:{message.CurrencySymbol}"];
            if (string.IsNullOrWhiteSpace(currencyId))
            {
                await PublishFailure(message, $"No CoinPayments currency ID configured for {message.CurrencySymbol}.");
                return;
            }

            // 1. Create spend request
            var spendParams = new SpendRequestParams
            {
                ToAddress = message.WalletAddress,
                ToCurrency = currencyId,
                Amount = message.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                AmountCurrency = _configuration["CoinPayments:UsdCurrencyId"] ?? "5057",
                Memo = $"Withdrawal {message.RequestGuid}",
            };

            var spendResponse = await _coinPaymentProvider.CreateSpendRequestAsync(
                merchantWalletId, spendParams, context.CancellationToken);

            if (!spendResponse.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "CoinPayments CreateSpendRequest failed for {RequestGuid}: {StatusCode} - {Content}",
                    message.RequestGuid, spendResponse.StatusCode, spendResponse.Content);
                await PublishFailure(message, $"CoinPayments spend request failed: {spendResponse.Content}");
                return;
            }

            // 2. Parse spend request result
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

            // 3. Confirm the spend request (publish to blockchain)
            var confirmResponse = await _coinPaymentProvider.ConfirmSpendRequestAsync(
                merchantWalletId, spendRequestId, context.CancellationToken);

            if (!confirmResponse.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "CoinPayments ConfirmSpendRequest failed for {RequestGuid}: {StatusCode} - {Content}",
                    message.RequestGuid, confirmResponse.StatusCode, confirmResponse.Content);
                await PublishFailure(message, $"CoinPayments spend confirmation failed: {confirmResponse.Content}");
                return;
            }

            // 4. Publish success event back to Wallet
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
