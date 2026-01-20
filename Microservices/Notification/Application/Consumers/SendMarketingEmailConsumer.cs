using CryptoJackpot.Domain.Core.IntegrationEvents.Notification;
using CryptoJackpot.Notification.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Consumers;

/// <summary>
/// Consumer that processes individual marketing email events.
/// Kafka allows multiple consumers to process emails in parallel for high throughput.
/// 
/// Features:
/// - Automatic retries via Kafka consumer groups
/// - Rate limiting can be added at SMTP provider level
/// - Scales horizontally with more Notification service instances
/// </summary>
public class SendMarketingEmailConsumer : IConsumer<SendMarketingEmailEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<SendMarketingEmailConsumer> _logger;

    public SendMarketingEmailConsumer(
        IMediator mediator,
        ILogger<SendMarketingEmailConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendMarketingEmailEvent> context)
    {
        var message = context.Message;
        
        _logger.LogDebug(
            "Processing marketing email for campaign {CampaignId}, batch {BatchNumber}, recipient {Email}",
            message.CampaignId, message.BatchNumber, message.Email);

        try
        {
            var result = await _mediator.Send(new SendLotteryMarketingEmailCommand
            {
                Email = message.Email,
                UserName = message.UserName,
                UserLastName = message.UserLastName,
                LotteryId = message.LotteryId,
                LotteryTitle = message.LotteryTitle,
                LotteryDescription = message.LotteryDescription,
                TicketPrice = message.TicketPrice,
                StartDate = message.StartDate,
                EndDate = message.EndDate,
                MaxTickets = message.MaxTickets
            });

            if (result.IsSuccess)
            {
                _logger.LogDebug(
                    "Successfully sent marketing email to {Email} for campaign {CampaignId}",
                    message.Email, message.CampaignId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to send marketing email to {Email} for campaign {CampaignId}: {Errors}",
                    message.Email, message.CampaignId, string.Join(", ", result.Errors.Select(e => e.Message)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending marketing email to {Email} for campaign {CampaignId}",
                message.Email, message.CampaignId);
            
            // Rethrow to let MassTransit handle retries
            throw;
        }
    }
}
