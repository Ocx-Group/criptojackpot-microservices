using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Notification.Application.Commands;
using CryptoJackpot.Notification.Application.Configuration;
using CryptoJackpot.Notification.Application.Constants;
using CryptoJackpot.Notification.Application.Interfaces;
using CryptoJackpot.Notification.Domain.Interfaces;
using CryptoJackpot.Notification.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoJackpot.Notification.Application.Handlers.Commands;

public class SendReferralBonusNotificationHandler : IRequestHandler<SendReferralBonusNotificationCommand, Result<bool>>
{
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly NotificationConfiguration _config;
    private readonly ILogger<SendReferralBonusNotificationHandler> _logger;

    public SendReferralBonusNotificationHandler(
        IEmailTemplateProvider templateProvider,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        IOptions<NotificationConfiguration> config,
        ILogger<SendReferralBonusNotificationHandler> logger)
    {
        _templateProvider = templateProvider;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendReferralBonusNotificationCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateProvider.GetTemplateAsync(TemplateNames.ReferralBonusCredited);
        if (template == null)
        {
            _logger.LogError("Template not found: {TemplateName}", TemplateNames.ReferralBonusCredited);
            return Result.Fail<bool>(new NotFoundError($"Template not found: {TemplateNames.ReferralBonusCredited}"));
        }

        var referrerFullName  = $"{request.ReferrerName} {request.ReferrerLastName}";
        var referredFullName  = $"{request.ReferredName} {request.ReferredLastName}";
        var walletUrl         = $"{_config.Brevo!.BaseUrl}{UrlPaths.MyWallet}";
        var referralsUrl      = $"{_config.Brevo!.BaseUrl}{UrlPaths.ReferralProgram}";
        var transactionUrl    = $"{_config.Brevo!.BaseUrl}{UrlPaths.Transactions}";

        var body = template
            .Replace("{ReferrerName}",    referrerFullName)
            .Replace("{ReferredName}",    referredFullName)
            .Replace("{ReferralCode}",    request.ReferralCode)
            .Replace("{BonusAmount}",     request.BonusAmount.ToString("F2"))
            .Replace("{BalanceAfter}",    request.BalanceAfter.ToString("F2"))
            .Replace("{TransactionId}",   request.TransactionGuid.ToString()[..8].ToUpper())
            .Replace("{CreditedAt}",      request.CreditedAt.ToString("MMM dd, yyyy HH:mm 'UTC'"))
            .Replace("{WalletUrl}",       walletUrl)
            .Replace("{ReferralsUrl}",    referralsUrl)
            .Replace("{TransactionUrl}",  transactionUrl);

        var subject = $"You earned ${request.BonusAmount:F2} — Referral Bonus Credited!";
        var emailResult = await _emailProvider.SendEmailAsync(request.ReferrerEmail, subject, body);

        await _logRepository.AddAsync(new NotificationLog
        {
            Type = "Email",
            Recipient = request.ReferrerEmail,
            Subject = subject,
            TemplateName = TemplateNames.ReferralBonusCredited,
            Success = emailResult.Success,
            ErrorMessage = emailResult.ErrorMessage,
            SentAt = DateTime.UtcNow
        });

        if (!emailResult.Success)
        {
            _logger.LogWarning("Failed to send referral bonus notification to {Email}. Error: {Error}",
                request.ReferrerEmail, emailResult.ErrorMessage);
            return Result.Fail<bool>(new InternalServerError("Failed to send referral bonus notification email"));
        }

        _logger.LogInformation("Referral bonus notification sent to {Email} — Amount: ${Amount}",
            request.ReferrerEmail, request.BonusAmount);
        return Result.Ok(true);
    }
}

