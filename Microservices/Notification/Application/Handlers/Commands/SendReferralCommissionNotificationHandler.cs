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

public class SendReferralCommissionNotificationHandler : IRequestHandler<SendReferralCommissionNotificationCommand, Result<bool>>
{
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly NotificationConfiguration _config;
    private readonly ILogger<SendReferralCommissionNotificationHandler> _logger;

    public SendReferralCommissionNotificationHandler(
        IEmailTemplateProvider templateProvider,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        IOptions<NotificationConfiguration> config,
        ILogger<SendReferralCommissionNotificationHandler> logger)
    {
        _templateProvider = templateProvider;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendReferralCommissionNotificationCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateProvider.GetTemplateAsync(TemplateNames.ReferralCommissionCredited);
        if (template == null)
        {
            _logger.LogError("Template not found: {TemplateName}", TemplateNames.ReferralCommissionCredited);
            return Result.Fail<bool>(new NotFoundError($"Template not found: {TemplateNames.ReferralCommissionCredited}"));
        }

        var referrerFullName = $"{request.ReferrerName} {request.ReferrerLastName}";
        var walletUrl        = $"{_config.Brevo!.BaseUrl}{UrlPaths.MyWallet}";
        var referralsUrl     = $"{_config.Brevo!.BaseUrl}{UrlPaths.ReferralProgram}";
        var transactionUrl   = $"{_config.Brevo!.BaseUrl}{UrlPaths.Transactions}";

        var body = template
            .Replace("{ReferrerName}",    referrerFullName)
            .Replace("{BuyerName}",       request.BuyerName)
            .Replace("{LotteryTitle}",    request.LotteryTitle)
            .Replace("{CommissionAmount}", request.CommissionAmount.ToString("F2"))
            .Replace("{BalanceAfter}",    request.BalanceAfter.ToString("F2"))
            .Replace("{TransactionId}",   request.TransactionGuid.ToString()[..8].ToUpper())
            .Replace("{OrderId}",         request.OrderId.ToString()[..8].ToUpper())
            .Replace("{CreditedAt}",      request.CreditedAt.ToString("MMM dd, yyyy HH:mm 'UTC'"))
            .Replace("{WalletUrl}",       walletUrl)
            .Replace("{ReferralsUrl}",    referralsUrl)
            .Replace("{TransactionUrl}",  transactionUrl);

        var subject = $"You earned ${request.CommissionAmount:F2} — Referral Commission Credited!";
        var emailResult = await _emailProvider.SendEmailAsync(request.ReferrerEmail, subject, body);

        await _logRepository.AddAsync(new NotificationLog
        {
            Type = "Email",
            Recipient = request.ReferrerEmail,
            Subject = subject,
            TemplateName = TemplateNames.ReferralCommissionCredited,
            Success = emailResult.Success,
            ErrorMessage = emailResult.ErrorMessage,
            SentAt = DateTime.UtcNow
        });

        if (!emailResult.Success)
        {
            _logger.LogWarning("Failed to send referral commission notification to {Email}. Error: {Error}",
                request.ReferrerEmail, emailResult.ErrorMessage);
            return Result.Fail<bool>(new InternalServerError("Failed to send referral commission notification email"));
        }

        _logger.LogInformation("Referral commission notification sent to {Email} — Amount: ${Amount}",
            request.ReferrerEmail, request.CommissionAmount);
        return Result.Ok(true);
    }
}

