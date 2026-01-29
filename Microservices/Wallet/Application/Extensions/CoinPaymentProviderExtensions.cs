using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Wallet.Application.DTOs.CoinPayments;
using CryptoJackpot.Wallet.Domain.Interfaces;

namespace CryptoJackpot.Wallet.Application.Extensions;

/// <summary>
/// Extension methods for ICoinPaymentProvider to provide strongly-typed API calls
/// </summary>
public static class CoinPaymentProviderExtensions
{
    /// <summary>
    /// Gets a deposit address for a specific cryptocurrency
    /// </summary>
    public static async Task<CoinPaymentsApiResponse<GetCallbackAddressResult>?> GetCallbackAddressAsync(
        this ICoinPaymentProvider provider,
        string currency,
        string? ipnUrl = null,
        CancellationToken cancellationToken = default)
    {
        var parms = new SortedList<string, string>
        {
            ["currency"] = currency
        };

        if (!string.IsNullOrEmpty(ipnUrl))
            parms["ipn_url"] = ipnUrl;

        var response = await provider.CallApiAsync("get_callback_address", parms, cancellationToken);
        return response.Deserialize<CoinPaymentsApiResponse<GetCallbackAddressResult>>();
    }

    /// <summary>
    /// Creates a new transaction for receiving cryptocurrency
    /// </summary>
    public static async Task<CoinPaymentsApiResponse<CreateTransactionResult>?> CreateTransactionAsync(
        this ICoinPaymentProvider provider,
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var parms = new SortedList<string, string>
        {
            ["amount"] = request.Amount.ToString("F8", System.Globalization.CultureInfo.InvariantCulture),
            ["currency1"] = request.CurrencyFrom,
            ["currency2"] = request.CurrencyTo
        };

        if (!string.IsNullOrEmpty(request.BuyerEmail))
            parms["buyer_email"] = request.BuyerEmail;

        if (!string.IsNullOrEmpty(request.BuyerName))
            parms["buyer_name"] = request.BuyerName;

        if (!string.IsNullOrEmpty(request.ItemName))
            parms["item_name"] = request.ItemName;

        if (!string.IsNullOrEmpty(request.IpnUrl))
            parms["ipn_url"] = request.IpnUrl;

        var response = await provider.CallApiAsync("create_transaction", parms, cancellationToken);
        return response.Deserialize<CoinPaymentsApiResponse<CreateTransactionResult>>();
    }

    /// <summary>
    /// Gets information about a specific transaction
    /// </summary>
    public static async Task<CoinPaymentsApiResponse<TransactionInfoResult>?> GetTransactionInfoAsync(
        this ICoinPaymentProvider provider,
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        var parms = new SortedList<string, string>
        {
            ["txid"] = transactionId
        };

        var response = await provider.CallApiAsync("get_tx_info", parms, cancellationToken);
        return response.Deserialize<CoinPaymentsApiResponse<TransactionInfoResult>>();
    }

    /// <summary>
    /// Gets account balances for all currencies or a specific one
    /// </summary>
    public static async Task<CoinPaymentsApiResponse<Dictionary<string, BalanceResult>>?> GetBalancesAsync(
        this ICoinPaymentProvider provider,
        bool includeAll = false,
        CancellationToken cancellationToken = default)
    {
        var parms = new SortedList<string, string>
        {
            ["all"] = includeAll ? "1" : "0"
        };

        var response = await provider.CallApiAsync("balances", parms, cancellationToken);
        return response.Deserialize<CoinPaymentsApiResponse<Dictionary<string, BalanceResult>>>();
    }

    /// <summary>
    /// Gets current exchange rates
    /// </summary>
    public static async Task<CoinPaymentsApiResponse<Dictionary<string, RateResult>>?> GetRatesAsync(
        this ICoinPaymentProvider provider,
        bool acceptedOnly = true,
        CancellationToken cancellationToken = default)
    {
        var parms = new SortedList<string, string>
        {
            ["accepted"] = acceptedOnly ? "1" : "0"
        };

        var response = await provider.CallApiAsync("rates", parms, cancellationToken);
        return response.Deserialize<CoinPaymentsApiResponse<Dictionary<string, RateResult>>>();
    }

    /// <summary>
    /// Creates a withdrawal to a specific address
    /// </summary>
    public static async Task<RestResponse> CreateWithdrawalAsync(
        this ICoinPaymentProvider provider,
        decimal amount,
        string currency,
        string address,
        bool autoConfirm = false,
        string? ipnUrl = null,
        CancellationToken cancellationToken = default)
    {
        var parms = new SortedList<string, string>
        {
            ["amount"] = amount.ToString("F8"),
            ["currency"] = currency,
            ["address"] = address,
            ["auto_confirm"] = autoConfirm ? "1" : "0"
        };

        if (!string.IsNullOrEmpty(ipnUrl))
            parms["ipn_url"] = ipnUrl;

        return await provider.CallApiAsync("create_withdrawal", parms, cancellationToken);
    }
}