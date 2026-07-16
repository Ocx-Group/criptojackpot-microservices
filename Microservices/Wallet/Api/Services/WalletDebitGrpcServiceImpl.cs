using CryptoJackpot.Domain.Core.Protos;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using Grpc.Core;

namespace CryptoJackpot.Wallet.Api.Services;

/// <summary>
/// gRPC server implementation for wallet debit operations.
/// Allows the Order microservice to debit internal balance for ticket purchases.
/// </summary>
public class WalletDebitGrpcServiceImpl : WalletDebitGrpcService.WalletDebitGrpcServiceBase
{
    private readonly IWalletService _walletService;
    private readonly ILogger<WalletDebitGrpcServiceImpl> _logger;

    public WalletDebitGrpcServiceImpl(
        IWalletService walletService,
        ILogger<WalletDebitGrpcServiceImpl> logger)
    {
        _walletService = walletService;
        _logger = logger;
    }

    public override async Task<DebitBalanceResponse> DebitBalance(
        DebitBalanceRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserGuid, out var userGuid))
        {
            _logger.LogWarning("Invalid user GUID received: {UserGuid}", request.UserGuid);
            return new DebitBalanceResponse
            {
                Success = false,
                ErrorMessage = "Invalid user GUID format"
            };
        }

        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            _logger.LogWarning("Invalid order ID received: {OrderId}", request.OrderId);
            return new DebitBalanceResponse
            {
                Success = false,
                ErrorMessage = "Invalid order ID format"
            };
        }

        var result = await _walletService.ApplyTransactionAsync(
            userGuid: userGuid,
            amount: (decimal)request.Amount,
            direction: WalletTransactionDirection.Debit,
            type: WalletTransactionType.TicketPurchase,
            referenceId: orderId,
            description: request.Description,
            cancellationToken: context.CancellationToken);

        if (result.IsFailed)
        {
            var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
            _logger.LogWarning(
                "Balance debit failed for user {UserGuid}, amount {Amount}: {Error}",
                userGuid, request.Amount, errorMessage);

            return new DebitBalanceResponse
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        _logger.LogInformation(
            "Balance debited: user {UserGuid}, amount {Amount}, order {OrderId}, tx {TxGuid}",
            userGuid, request.Amount, orderId, result.Value.TransactionGuid);

        return new DebitBalanceResponse
        {
            Success = true,
            TransactionGuid = result.Value.TransactionGuid.ToString(),
            BalanceAfter = (double)result.Value.BalanceAfter
        };
    }
}
