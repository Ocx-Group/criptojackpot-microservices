using System.Text;
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

public class SendPurchaseConfirmationHandler : IRequestHandler<SendPurchaseConfirmationCommand, Result<bool>>
{
    private readonly IEmailTemplateProvider _templateProvider;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly NotificationConfiguration _config;
    private readonly ILogger<SendPurchaseConfirmationHandler> _logger;

    public SendPurchaseConfirmationHandler(
        IEmailTemplateProvider templateProvider,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        IOptions<NotificationConfiguration> config,
        ILogger<SendPurchaseConfirmationHandler> logger)
    {
        _templateProvider = templateProvider;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendPurchaseConfirmationCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateProvider.GetTemplateAsync(TemplateNames.PurchaseConfirmation);
        if (template == null)
        {
            _logger.LogError("Template not found: {TemplateName}", TemplateNames.PurchaseConfirmation);
            return Result.Fail<bool>(new NotFoundError($"Template not found: {TemplateNames.PurchaseConfirmation}"));
        }

        var ticketRowsHtml = BuildTicketRowsHtml(request.Tickets);
        var lotteryUrl = $"{_config.Brevo!.BaseUrl}{UrlPaths.MyTickets}";

        var body = template
            .Replace("{UserName}", request.UserName)
            .Replace("{OrderId}", request.OrderId.ToString()[..8].ToUpper())
            .Replace("{LotteryTitle}", request.LotteryTitle)
            .Replace("{LotteryNo}", request.LotteryNo)
            .Replace("{TicketRows}", ticketRowsHtml)
            .Replace("{TicketCount}", request.Tickets.Count.ToString())
            .Replace("{TotalAmount}", request.TotalAmount.ToString("F2"))
            .Replace("{TransactionId}", request.TransactionId)
            .Replace("{PurchaseDate}", request.PurchaseDate.ToString("MMM dd, yyyy HH:mm 'UTC'"))
            .Replace("{LotteryUrl}", lotteryUrl);

        var subject = $"Purchase Confirmed - {request.Tickets.Count} Ticket(s) for {request.LotteryTitle}";
        var emailResult = await _emailProvider.SendEmailAsync(request.Email, subject, body);

        await _logRepository.AddAsync(new NotificationLog
        {
            Type = "Email",
            Recipient = request.Email,
            Subject = subject,
            TemplateName = TemplateNames.PurchaseConfirmation,
            Success = emailResult.Success,
            ErrorMessage = emailResult.ErrorMessage,
            SentAt = DateTime.UtcNow
        });

        if (!emailResult.Success)
        {
            _logger.LogWarning("Failed to send purchase confirmation to {Email}. Error: {Error}",
                request.Email, emailResult.ErrorMessage);
            return Result.Fail<bool>(new InternalServerError("Failed to send purchase confirmation email"));
        }

        _logger.LogInformation("Purchase confirmation sent to {Email} for order {OrderId}",
            request.Email, request.OrderId);
        return Result.Ok(true);
    }

    private static string BuildTicketRowsHtml(List<PurchasedTicketItemDto> tickets)
    {
        var sb = new StringBuilder();
        foreach (var ticket in tickets)
        {
            var giftBadge = ticket.IsGift
                ? " <span style=\"background-color:#FFA500;color:#fff;padding:2px 6px;border-radius:3px;font-size:11px;\">GIFT</span>"
                : string.Empty;

            sb.AppendLine($"""
                <tr>
                    <td style="padding:10px 15px;border-bottom:1px solid #eee;text-align:center;font-size:18px;font-weight:bold;">{ticket.Number:D4}</td>
                    <td style="padding:10px 15px;border-bottom:1px solid #eee;text-align:center;">{ticket.Series}{giftBadge}</td>
                    <td style="padding:10px 15px;border-bottom:1px solid #eee;text-align:right;">${ticket.Amount:F2}</td>
                </tr>
            """);
        }

        return sb.ToString();
    }
}

