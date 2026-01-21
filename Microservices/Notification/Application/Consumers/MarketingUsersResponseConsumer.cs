using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Domain.Core.IntegrationEvents.Notification;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Consumers;

/// <summary>
/// Consumer that handles GetUsersForMarketingResponseEvent from Identity service.
/// Receives the list of users and queues individual marketing emails for each user.
/// This is the second step of the Saga pattern for async marketing campaigns.
/// </summary>
public class MarketingUsersResponseConsumer : IConsumer<GetUsersForMarketingResponseEvent>
{
    private const int BatchSize = 100;

    private readonly IEventBus _eventBus;
    private readonly ILogger<MarketingUsersResponseConsumer> _logger;

    public MarketingUsersResponseConsumer(
        IEventBus eventBus,
        ILogger<MarketingUsersResponseConsumer> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetUsersForMarketingResponseEvent> context)
    {
        var response = context.Message;

        _logger.LogInformation(
            "Received GetUsersForMarketingResponseEvent. CorrelationId: {CorrelationId}, Success: {Success}, UserCount: {UserCount}",
            response.CorrelationId, response.Success, response.Users.Count);

        if (!response.Success)
        {
            _logger.LogError(
                "Marketing campaign {CorrelationId} failed: {ErrorMessage}",
                response.CorrelationId, response.ErrorMessage);
            return;
        }

        if (response.Users.Count == 0)
        {
            _logger.LogWarning(
                "Marketing campaign {CorrelationId}: No users found. Skipping email distribution.",
                response.CorrelationId);
            return;
        }

        try
        {
            // Publish individual email events in batches for parallel processing
            var batchNumber = 0;
            var totalQueued = 0;

            foreach (var batch in response.Users.Chunk(BatchSize))
            {
                batchNumber++;

                foreach (var user in batch)
                {
                    await _eventBus.Publish(new SendMarketingEmailEvent
                    {
                        Email = user.Email,
                        UserName = user.Name,
                        UserLastName = user.LastName,
                        LotteryId = response.LotteryId,
                        LotteryTitle = response.LotteryTitle,
                        LotteryDescription = response.LotteryDescription,
                        TicketPrice = response.TicketPrice,
                        StartDate = response.StartDate,
                        EndDate = response.EndDate,
                        MaxTickets = response.MaxTickets,
                        CampaignId = response.CorrelationId,
                        BatchNumber = batchNumber
                    });

                    totalQueued++;
                }

                _logger.LogDebug(
                    "Campaign {CorrelationId}: Queued batch {BatchNumber} ({Count} emails)",
                    response.CorrelationId, batchNumber, batch.Length);
            }

            _logger.LogInformation(
                "Campaign {CorrelationId}: Successfully queued {Total} marketing emails for lottery {LotteryId} in {Batches} batches",
                response.CorrelationId, totalQueued, response.LotteryId, batchNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Campaign {CorrelationId}: Error distributing marketing emails for lottery {LotteryId}",
                response.CorrelationId, response.LotteryId);
            throw;
        }
    }
}