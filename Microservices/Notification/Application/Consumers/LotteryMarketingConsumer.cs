using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Consumers;

/// <summary>
/// Consumer that handles LotteryCreatedEvent to initiate marketing campaign.
/// Publishes GetUsersForMarketingRequestEvent to Identity service via Kafka (Saga pattern).
/// The response will be handled by MarketingUsersResponseConsumer.
/// 
/// Flow:
/// 1. LotteryCreatedEvent received → Publish GetUsersForMarketingRequestEvent
/// 2. Identity service responds with GetUsersForMarketingResponseEvent
/// 3. MarketingUsersResponseConsumer queues individual emails
/// 
/// Scalability: Can handle 100,000+ users by distributing work via Kafka.
/// </summary>
public class LotteryMarketingConsumer : IConsumer<LotteryCreatedEvent>
{
    private readonly ILogger<LotteryMarketingConsumer> _logger;
    private readonly IEventBus _eventBus;

    public LotteryMarketingConsumer(
        IEventBus eventBus,
        ILogger<LotteryMarketingConsumer> logger)
    {
        _logger = logger;
        _eventBus = eventBus;
    }

    public async Task Consume(ConsumeContext<LotteryCreatedEvent> context)
    {
        var lottery = context.Message;
        var campaignId = Guid.NewGuid();

        _logger.LogInformation(
            "Received LotteryCreatedEvent for lottery {LotteryId} - {Title}. Starting marketing campaign {CampaignId}.",
            lottery.LotteryId, lottery.Title, campaignId);

        try
        {
            // Publish request event to Identity service via Kafka (Saga pattern)
            // The response will be handled by MarketingUsersResponseConsumer
            await _eventBus.Publish(new GetUsersForMarketingRequestEvent
            {
                CorrelationId = campaignId,
                LotteryId = lottery.LotteryId,
                LotteryTitle = lottery.Title,
                LotteryDescription = lottery.Description,
                TicketPrice = lottery.TicketPrice,
                StartDate = lottery.StartDate,
                EndDate = lottery.EndDate,
                MaxTickets = lottery.MaxTickets,
                OnlyActiveUsers = true // Status = true means email confirmed
            });

            _logger.LogInformation(
                "Campaign {CampaignId}: Published GetUsersForMarketingRequestEvent for lottery {LotteryId}. Waiting for Identity service response.",
                campaignId, lottery.LotteryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Campaign {CampaignId}: Error publishing GetUsersForMarketingRequestEvent for lottery {LotteryId}",
                campaignId, lottery.LotteryId);
            throw;
        }
    }
}