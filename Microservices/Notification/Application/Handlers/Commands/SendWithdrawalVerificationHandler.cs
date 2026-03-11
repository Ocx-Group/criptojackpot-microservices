using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Notification.Application.Commands;
using CryptoJackpot.Notification.Application.Constants;
using CryptoJackpot.Notification.Application.Interfaces;
using CryptoJackpot.Notification.Domain.Interfaces;
using CryptoJackpot.Notification.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Handlers.Commands;

public class SendWithdrawalVerificationHandler : IRequestHandler<SendWithdrawalVerificationCommand, Result<bool>>
{
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly ILogger<SendWithdrawalVerificationHandler> _logger;

    public SendWithdrawalVerificationHandler(
        IEmailTemplateProvider templateProvider,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        ILogger<SendWithdrawalVerificationHandler> logger)
    {
        _templateProvider = templateProvider;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendWithdrawalVerificationCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateProvider.GetTemplateAsync(TemplateNames.WithdrawalVerification);
        if (template == null)
        {
            _logger.LogError("Template not found: {TemplateName}", TemplateNames.WithdrawalVerification);
            return Result.Fail<bool>(new NotFoundError($"Template not found: {TemplateNames.WithdrawalVerification}"));
        }

        var fullName = $"{request.Name} {request.LastName}";
        var body = template
            .Replace("{0}", fullName)
            .Replace("{1}", request.SecurityCode)
            .Replace("{2}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));

        var subject = "Withdrawal Verification - CryptoJackpot";
        var emailResult = await _emailProvider.SendEmailAsync(request.Email, subject, body);

        await _logRepository.AddAsync(new NotificationLog
        {
            Type = "Email",
            Recipient = request.Email,
            Subject = subject,
            TemplateName = TemplateNames.WithdrawalVerification,
            Success = emailResult.Success,
            ErrorMessage = emailResult.ErrorMessage,
            SentAt = DateTime.UtcNow
        });

        if (!emailResult.Success)
        {
            _logger.LogWarning("Failed to send withdrawal verification email to {Email}. Error: {Error}",
                request.Email, emailResult.ErrorMessage);
            return Result.Fail<bool>(new InternalServerError("Failed to send email"));
        }

        _logger.LogInformation("Withdrawal verification email sent successfully to {Email}", request.Email);
        return Result.Ok(true);
    }
}
