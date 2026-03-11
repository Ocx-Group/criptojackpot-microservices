using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Consumers;

/// <summary>
/// Consumes <see cref="ReferralCreatedEvent"/> from Identity service via Kafka.
/// Credits $5.00 USD referral bonus to the referrer's internal wallet.
/// After a successful credit, publishes <see cref="ReferralBonusCreditedEvent"/> so
/// the Notification service can send a confirmation email to the referrer.
/// Referral relationships are NOT stored locally — Identity is the single source of truth
/// and is queried via gRPC when needed (e.g., purchase commission calculation).
/// </summary>
public class ReferralCreatedConsumer : IConsumer<ReferralCreatedEvent>
{
    private const decimal ReferralBonusAmount = 5.00m;

    private readonly IWalletService _walletService;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ReferralCreatedConsumer> _logger;

    public ReferralCreatedConsumer(
        IWalletService walletService,
        IEventBus eventBus,
        ILogger<ReferralCreatedConsumer> logger)
    {
        _walletService = walletService;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReferralCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received ReferralCreatedEvent — Referrer: {ReferrerGuid}, Referred: {ReferredGuid}, Code: {Code}",
            message.ReferrerUserGuid, message.ReferredUserGuid, message.ReferralCode);

        // ── Credit referral bonus ───────────────────────────────────────
        var description = $"Referral bonus — {message.ReferredName} {message.ReferredLastName} joined with code {message.ReferralCode}";

        var result = await _walletService.ApplyTransactionAsync(
            userGuid: message.ReferrerUserGuid,
            amount: ReferralBonusAmount,
            direction: WalletTransactionDirection.Credit,
            type: WalletTransactionType.ReferralBonus,
            referenceId: null,
            description: description,
            cancellationToken: context.CancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Referral bonus credited — Referred: {ReferredGuid} → Referrer: {ReferrerGuid}, Bonus: {Amount} USD, Tx: {TxGuid}",
                message.ReferredUserGuid, message.ReferrerUserGuid, ReferralBonusAmount, result.Value.TransactionGuid);

            // ── Notify referrer via email ───────────────────────────────
            try
            {
                await _eventBus.Publish(new ReferralBonusCreditedEvent
                {
                    ReferrerUserGuid  = message.ReferrerUserGuid,
                    ReferrerEmail     = message.ReferrerEmail,
                    ReferrerName      = message.ReferrerName,
                    ReferrerLastName  = message.ReferrerLastName,
                    ReferredName      = message.ReferredName,
                    ReferredLastName  = message.ReferredLastName,
                    ReferralCode      = message.ReferralCode,
                    BonusAmount       = ReferralBonusAmount,
                    BalanceAfter      = result.Value.BalanceAfter,
                    TransactionGuid   = result.Value.TransactionGuid,
                    CreditedAt        = DateTime.UtcNow
                });

                _logger.LogInformation(
                    "ReferralBonusCreditedEvent published for referrer {ReferrerGuid}",
                    message.ReferrerUserGuid);
            }
            catch (Exception ex)
            {
                // Non-critical: wallet credit already succeeded; log and continue.
                _logger.LogError(ex,
                    "Failed to publish ReferralBonusCreditedEvent for referrer {ReferrerGuid}",
                    message.ReferrerUserGuid);
            }
        }
        else
        {
            _logger.LogError(
                "Failed to credit referral bonus to referrer {ReferrerGuid}. Errors: {Errors}",
                message.ReferrerUserGuid, string.Join("; ", result.Errors.Select(e => e.Message)));
        }
    }
}

