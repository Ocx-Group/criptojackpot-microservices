using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Order;
using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Consumers;

/// <summary>
/// Consumes <see cref="OrderCompletedEvent"/> from Order service via Kafka.
/// Credits 1% of the ticket purchase amount to the referrer's wallet
/// as a <see cref="WalletTransactionType.ReferralPurchaseCommission"/>.
/// Resolves the referrer via gRPC call to Identity (single source of truth).
/// If the buyer has no referrer, the event is silently skipped.
/// </summary>
public class OrderCompletedConsumer : IConsumer<OrderCompletedEvent>
{
    /// <summary>Commission rate applied to the total order amount.</summary>
    private const decimal CommissionRate = 0.01m; // 1%

    private readonly IReferralGrpcClient _referralGrpcClient;
    private readonly IUserVerificationGrpcClient _userVerificationGrpcClient;
    private readonly IWalletService _walletService;
    private readonly IWalletRepository _walletRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrderCompletedConsumer> _logger;

    public OrderCompletedConsumer(
        IReferralGrpcClient referralGrpcClient,
        IUserVerificationGrpcClient userVerificationGrpcClient,
        IWalletService walletService,
        IWalletRepository walletRepository,
        IEventBus eventBus,
        ILogger<OrderCompletedConsumer> logger)
    {
        _referralGrpcClient = referralGrpcClient;
        _userVerificationGrpcClient = userVerificationGrpcClient;
        _walletService = walletService;
        _walletRepository = walletRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received OrderCompletedEvent — OrderId: {OrderId}, Buyer: {BuyerGuid}, Amount: {Amount}",
            message.OrderId, message.BuyerUserGuid, message.TotalAmount);

        // ── 1. Query Identity (source of truth) for the referrer ────────
        var referrerUserGuid = await _referralGrpcClient
            .GetReferrerUserGuidAsync(message.BuyerUserGuid, context.CancellationToken);

        if (referrerUserGuid is null)
        {
            _logger.LogDebug(
                "Buyer {BuyerGuid} has no referrer. Skipping commission for order {OrderId}",
                message.BuyerUserGuid, message.OrderId);
            return;
        }

        // ── 2. Calculate commission ─────────────────────────────────────
        var commission = Math.Round(message.TotalAmount * CommissionRate, 4);

        if (commission <= 0)
        {
            _logger.LogWarning(
                "Calculated commission is zero or negative for order {OrderId}. TotalAmount: {Amount}",
                message.OrderId, message.TotalAmount);
            return;
        }

        // ── 3. Idempotency: check if commission for this order was already credited ──
        var existingTx = await _walletRepository.GetByReferenceIdAndTypeAsync(
            message.OrderId, WalletTransactionType.ReferralPurchaseCommission, context.CancellationToken);

        if (existingTx is not null)
        {
            _logger.LogInformation(
                "Commission already credited for order {OrderId}. Transaction: {TxGuid}. Skipping (idempotent)",
                message.OrderId, existingTx.TransactionGuid);
            return;
        }

        // ── 4. Credit the referrer ──────────────────────────────────────
        var description = $"Referral commission (1%) — {message.UserName} purchased tickets in {message.LotteryTitle}";

        var result = await _walletService.ApplyTransactionAsync(
            userGuid: referrerUserGuid.Value,
            amount: commission,
            direction: WalletTransactionDirection.Credit,
            type: WalletTransactionType.ReferralPurchaseCommission,
            referenceId: message.OrderId,
            description: description,
            cancellationToken: context.CancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Referral commission of {Commission} USD credited to referrer {ReferrerGuid} for order {OrderId}. Transaction: {TxGuid}",
                commission, referrerUserGuid.Value, message.OrderId, result.Value.TransactionGuid);

            // ── 5. Notify referrer via email ────────────────────────────
            try
            {
                var referrerInfo = await _userVerificationGrpcClient
                    .GetUserInfoAsync(referrerUserGuid.Value, context.CancellationToken);

                if (referrerInfo is not null)
                {
                    await _eventBus.Publish(new ReferralCommissionCreditedEvent
                    {
                        ReferrerUserGuid = referrerUserGuid.Value,
                        ReferrerEmail    = referrerInfo.Email,
                        ReferrerName     = referrerInfo.Name,
                        ReferrerLastName = referrerInfo.LastName,
                        BuyerName        = message.UserName,
                        LotteryTitle     = message.LotteryTitle,
                        CommissionAmount = commission,
                        BalanceAfter     = result.Value.BalanceAfter,
                        TransactionGuid  = result.Value.TransactionGuid,
                        OrderId          = message.OrderId,
                        CreditedAt       = DateTime.UtcNow
                    });

                    _logger.LogInformation(
                        "ReferralCommissionCreditedEvent published for referrer {ReferrerGuid}",
                        referrerUserGuid.Value);
                }
                else
                {
                    _logger.LogWarning(
                        "Could not resolve referrer info via gRPC for {ReferrerGuid}. Commission was credited but notification skipped",
                        referrerUserGuid.Value);
                }
            }
            catch (Exception ex)
            {
                // Non-critical: wallet credit already succeeded; log and continue.
                _logger.LogError(ex,
                    "Failed to publish ReferralCommissionCreditedEvent for referrer {ReferrerGuid}",
                    referrerUserGuid.Value);
            }
        }
        else
        {
            _logger.LogError(
                "Failed to credit referral commission to referrer {ReferrerGuid} for order {OrderId}. Errors: {Errors}",
                referrerUserGuid.Value, message.OrderId,
                string.Join("; ", result.Errors.Select(e => e.Message)));
        }
    }
}
