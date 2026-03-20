using CryptoJackpot.Domain.Core.Protos;
using CryptoJackpot.Order.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Services;

/// <summary>
/// gRPC client wrapper that calls Wallet microservice to debit internal balance.
/// </summary>
public class WalletDebitGrpcClient : IWalletDebitGrpcClient
{
    private readonly WalletDebitGrpcService.WalletDebitGrpcServiceClient _client;
    private readonly ILogger<WalletDebitGrpcClient> _logger;

    public WalletDebitGrpcClient(
        WalletDebitGrpcService.WalletDebitGrpcServiceClient client,
        ILogger<WalletDebitGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<WalletDebitResult> DebitBalanceAsync(
        Guid userGuid,
        decimal amount,
        Guid orderId,
        string description,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Requesting wallet debit: user {UserGuid}, amount {Amount}, order {OrderId}",
            userGuid, amount, orderId);

        var request = new DebitBalanceRequest
        {
            UserGuid = userGuid.ToString(),
            Amount = (double)amount,
            OrderId = orderId.ToString(),
            Description = description
        };

        var response = await _client.DebitBalanceAsync(
            request,
            cancellationToken: cancellationToken);

        if (!response.Success)
        {
            _logger.LogWarning(
                "Wallet debit rejected: user {UserGuid}, amount {Amount}, order {OrderId}. Reason: {Error}",
                userGuid, amount, orderId, response.ErrorMessage);
        }
        else
        {
            _logger.LogInformation(
                "Wallet debit succeeded: user {UserGuid}, amount {Amount}, order {OrderId}, tx {TxGuid}",
                userGuid, amount, orderId, response.TransactionGuid);
        }

        return new WalletDebitResult
        {
            Success = response.Success,
            TransactionGuid = response.TransactionGuid,
            ErrorMessage = response.ErrorMessage,
            BalanceAfter = (decimal)response.BalanceAfter
        };
    }
}
