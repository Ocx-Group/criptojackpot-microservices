using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Consumers;

/// <summary>
/// Consumes WithdrawalFailedEvent from Order microservice when a CoinPayments
/// spend fails. Reverts the withdrawal request status to Pending and refunds the user.
/// </summary>
public class WithdrawalFailedConsumer : IConsumer<WithdrawalFailedEvent>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IWalletService _walletService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WithdrawalFailedConsumer> _logger;

    public WithdrawalFailedConsumer(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IWalletService walletService,
        IUnitOfWork unitOfWork,
        ILogger<WithdrawalFailedConsumer> logger)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _walletService = walletService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WithdrawalFailedEvent> context)
    {
        var message = context.Message;

        _logger.LogWarning(
            "Processing WithdrawalFailedEvent for {RequestGuid}: {Reason}",
            message.RequestGuid, message.Reason);

        var withdrawalRequest = await _withdrawalRequestRepository.GetByGuidAsync(
            message.RequestGuid, context.CancellationToken);

        if (withdrawalRequest is null)
        {
            _logger.LogWarning(
                "Withdrawal request {RequestGuid} not found. Ignoring failure event.",
                message.RequestGuid);
            return;
        }

        if (withdrawalRequest.Status != WithdrawalRequestStatus.Approved)
        {
            _logger.LogWarning(
                "Withdrawal request {RequestGuid} is not in Approved status (current: {Status}). Ignoring.",
                message.RequestGuid, withdrawalRequest.Status);
            return;
        }

        // Revert status to Pending so admin can retry or reject
        withdrawalRequest.Status = WithdrawalRequestStatus.Pending;
        withdrawalRequest.AdminNotes = $"CoinPayments failed: {message.Reason}";
        withdrawalRequest.ProcessedAt = null;

        _withdrawalRequestRepository.Update(withdrawalRequest);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Withdrawal request {RequestGuid} reverted to Pending for user {UserGuid} due to spend failure.",
            message.RequestGuid, message.UserGuid);
    }
}
