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
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ReferralCreatedConsumer> _logger;

    public ReferralCreatedConsumer(
        IWalletService walletService,
        IReferralRelationshipRepository referralRelationshipRepository,
        IUnitOfWork uow,
        ILogger<ReferralCreatedConsumer> logger)
    {
        _walletService = walletService;
        _referralRelationshipRepository = referralRelationshipRepository;
        _uow = uow;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReferralCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received ReferralCreatedEvent — Referrer: {ReferrerGuid}, Referred: {ReferredGuid}, Code: {Code}",
            message.ReferrerUserGuid, message.ReferredUserGuid, message.ReferralCode);

        // ── Persist referral relationship (idempotent) ──────────────────
        // The entity is added to the EF change tracker. It will be persisted
        // atomically when ApplyTransactionAsync calls SaveChangesAsync via UoW.
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
                "Referral relationship and bonus persisted — Referred: {ReferredGuid} → Referrer: {ReferrerGuid}, Bonus: {Amount} USD, Tx: {TxGuid}",
                message.ReferredUserGuid, message.ReferrerUserGuid, ReferralBonusAmount, result.Value.TransactionGuid);
        }
        else
        {
            // ApplyTransactionAsync failed — still persist the relationship so commissions work
            if (!alreadyExists)
            {
                await _uow.SaveChangesAsync(context.CancellationToken);
                _logger.LogWarning(
                    "Referral relationship persisted (fallback) for {ReferredGuid} → {ReferrerGuid}, but bonus credit failed: {Errors}",
                    message.ReferredUserGuid, message.ReferrerUserGuid,
                    string.Join("; ", result.Errors.Select(e => e.Message)));
            }
            else
            {
                _logger.LogError(
                    "Failed to credit referral bonus to referrer {ReferrerGuid}. Errors: {Errors}",
                    message.ReferrerUserGuid, string.Join("; ", result.Errors.Select(e => e.Message)));
            }
        }
    }
}

