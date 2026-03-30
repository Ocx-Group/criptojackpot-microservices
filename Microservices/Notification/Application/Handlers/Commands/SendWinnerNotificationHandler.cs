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

public class SendWinnerNotificationHandler : IRequestHandler<SendWinnerNotificationCommand, Result<bool>>
{
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly NotificationConfiguration _config;
    private readonly ILogger<SendWinnerNotificationHandler> _logger;

    public SendWinnerNotificationHandler(
        IEmailTemplateProvider templateProvider,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        IOptions<NotificationConfiguration> config,
        ILogger<SendWinnerNotificationHandler> logger)
    {
        _templateProvider = templateProvider;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendWinnerNotificationCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateProvider.GetTemplateAsync(TemplateNames.WinnerNotification);
        if (template == null)
        {
            _logger.LogError("Template not found: {TemplateName}", TemplateNames.WinnerNotification);
            return Result.Fail<bool>(new NotFoundError($"Template not found: {TemplateNames.WinnerNotification}"));
        }

        var myTicketsUrl = $"{_config.Brevo!.BaseUrl}{UrlPaths.MyTickets}";
        var prizeValue = request.PrizeEstimatedValue.HasValue
            ? $"${request.PrizeEstimatedValue.Value:F2}"
            : "TBD";

        var formattedNumber = request.LotteryType == 5
            ? request.Number.ToString("D3")
            : request.Number.ToString();

        var body = template
            .Replace("{UserName}", request.UserName)
            .Replace("{LotteryTitle}", request.LotteryTitle)
            .Replace("{WinnerRef}", request.WinnerGuid.ToString()[..8].ToUpper())
            .Replace("{Number}", formattedNumber)
            .Replace("{Series}", request.Series.ToString().PadLeft(2, '0'))
            .Replace("{PrizeName}", request.PrizeName ?? "Grand Prize")
            .Replace("{PrizeEstimatedValue}", prizeValue)
            .Replace("{PurchaseAmount}", request.PurchaseAmount.ToString("F2"))
            .Replace("{WonAt}", request.WonAt.ToString("MMM dd, yyyy HH:mm 'UTC'"))
            .Replace("{MyTicketsUrl}", myTicketsUrl);

        var subject = $"🏆 Congratulations! You Won {request.LotteryTitle}!";
        var emailResult = await _emailProvider.SendEmailAsync(request.Email, subject, body);

        await _logRepository.AddAsync(new NotificationLog
        {
            Type = "Email",
            Recipient = request.Email,
            Subject = subject,
            TemplateName = TemplateNames.WinnerNotification,
            Success = emailResult.Success,
            ErrorMessage = emailResult.ErrorMessage,
            SentAt = DateTime.UtcNow
        });

        if (!emailResult.Success)
        {
            _logger.LogWarning("Failed to send winner notification to {Email} for winner {WinnerGuid}. Error: {Error}",
                request.Email, request.WinnerGuid, emailResult.ErrorMessage);
            return Result.Fail<bool>(new InternalServerError("Failed to send winner notification email"));
        }

        _logger.LogInformation("Winner notification sent to {Email} for winner {WinnerGuid}",
            request.Email, request.WinnerGuid);
        return Result.Ok(true);
    }
}

