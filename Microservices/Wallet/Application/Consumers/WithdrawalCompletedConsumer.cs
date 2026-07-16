using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Consumers;

/// <summary>
/// Consumes WithdrawalCompletedEvent from Order microservice after a
/// successful CoinPayments spend. Updates the withdrawal request status to Completed.
/// </summary>
public class WithdrawalCompletedConsumer : IConsumer<WithdrawalCompletedEvent>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WithdrawalCompletedConsumer> _logger;

    public WithdrawalCompletedConsumer(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IUnitOfWork unitOfWork,
        ILogger<WithdrawalCompletedConsumer> logger)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WithdrawalCompletedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing WithdrawalCompletedEvent for {RequestGuid}, TxId: {TransactionId}",
            message.RequestGuid, message.TransactionId);

        var withdrawalRequest = await _withdrawalRequestRepository.GetByGuidAsync(
            message.RequestGuid, context.CancellationToken);

        if (withdrawalRequest is null)
        {
            _logger.LogWarning(
                "Withdrawal request {RequestGuid} not found. Ignoring completion event.",
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

        withdrawalRequest.Status = WithdrawalRequestStatus.Completed;
        _withdrawalRequestRepository.Update(withdrawalRequest);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Withdrawal request {RequestGuid} marked as Completed for user {UserGuid}: ${Amount}",
            message.RequestGuid, message.UserGuid, message.Amount);
    }
}
