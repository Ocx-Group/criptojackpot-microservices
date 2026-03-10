using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using CryptoJackpot.Wallet.Domain.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Consumers;

/// <summary>
/// Consumes <see cref="ReferralCreatedEvent"/> from Identity service via Kafka.
/// Credits $5.00 USD referral bonus to the referrer's internal wallet
/// and persists the referral relationship for future purchase commission calculations.
/// </summary>
public class ReferralCreatedConsumer : IConsumer<ReferralCreatedEvent>
{
    private const decimal ReferralBonusAmount = 5.00m;

    private readonly IWalletService _walletService;
    private readonly IReferralRelationshipRepository _referralRelationshipRepository;
    private readonly ILogger<ReferralCreatedConsumer> _logger;

    public ReferralCreatedConsumer(
        IWalletService walletService,
        IReferralRelationshipRepository referralRelationshipRepository,
        ILogger<ReferralCreatedConsumer> logger)
    {
        _walletService = walletService;
        _referralRelationshipRepository = referralRelationshipRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReferralCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received ReferralCreatedEvent — Referrer: {ReferrerGuid}, Referred: {ReferredGuid}, Code: {Code}",
            message.ReferrerUserGuid, message.ReferredUserGuid, message.ReferralCode);

        // ── Persist referral relationship (idempotent) ──────────────────
        var alreadyExists = await _referralRelationshipRepository
            .ExistsByReferredUserGuidAsync(message.ReferredUserGuid, context.CancellationToken);

        if (!alreadyExists)
        {
            await _referralRelationshipRepository.AddAsync(new ReferralRelationship
            {
                ReferrerUserGuid = message.ReferrerUserGuid,
                ReferredUserGuid = message.ReferredUserGuid,
                ReferralCode = message.ReferralCode
            }, context.CancellationToken);

            _logger.LogInformation(
                "Persisted referral relationship: {ReferredGuid} → {ReferrerGuid}",
                message.ReferredUserGuid, message.ReferrerUserGuid);
        }

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
                "Referral bonus of {Amount} USD credited to referrer {ReferrerGuid}. Transaction: {TxGuid}",
                ReferralBonusAmount, message.ReferrerUserGuid, result.Value.TransactionGuid);
        }
        else
        {
            _logger.LogError(
                "Failed to credit referral bonus to referrer {ReferrerGuid}. Errors: {Errors}",
                message.ReferrerUserGuid, string.Join("; ", result.Errors.Select(e => e.Message)));
        }
    }
}

