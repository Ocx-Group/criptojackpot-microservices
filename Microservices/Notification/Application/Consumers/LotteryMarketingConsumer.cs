using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using CryptoJackpot.Domain.Core.IntegrationEvents.Notification;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Notification.Application.Consumers;

/// <summary>
/// Consumer that handles LotteryCreatedEvent to queue marketing emails to all users.
/// Uses MassTransit Request/Response to get users, then publishes individual email events
/// for parallel processing by SendMarketingEmailConsumer.
/// 
/// Scalability: Can handle 100,000+ users by distributing work via Kafka.
/// </summary>
public class LotteryMarketingConsumer : IConsumer<LotteryCreatedEvent>
{
    private const int BatchSize = 100;
    
    private readonly ILogger<LotteryMarketingConsumer> _logger;
    private readonly IEventBus _eventBus;
    private readonly IRequestClient<GetAllUsersRequest> _usersClient;

    public LotteryMarketingConsumer(
        IEventBus eventBus,
        IRequestClient<GetAllUsersRequest> usersClient,
        ILogger<LotteryMarketingConsumer> logger)
    {
        _logger = logger;
        _eventBus = eventBus;
        _usersClient = usersClient;
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
            // Request all active users from Identity service via MassTransit Request/Response
            var response = await _usersClient.GetResponse<GetAllUsersResponse>(
                new GetAllUsersRequest
                {
                    OnlyActiveUsers = true,
                    OnlyConfirmedEmails = true
                },
                context.CancellationToken,
                timeout: RequestTimeout.After(m: 2)); // 2 minute timeout for large user lists

            if (!response.Message.Success)
            {
                _logger.LogError("Failed to get users from Identity service: {Error}", response.Message.ErrorMessage);
                return;
            }

            var users = response.Message.Users.ToList();
            _logger.LogInformation(
                "Campaign {CampaignId}: Retrieved {Count} users for lottery {LotteryId}",
                campaignId, users.Count, lottery.LotteryId);

            if (users.Count == 0)
            {
                _logger.LogWarning("Campaign {CampaignId}: No users found. Skipping.", campaignId);
                return;
            }

            // Publish individual email events in batches for parallel processing
            var batchNumber = 0;
            var totalQueued = 0;

            foreach (var batch in users.Chunk(BatchSize))
            {
                batchNumber++;
                
                foreach (var user in batch)
                {
                    await _eventBus.Publish(new SendMarketingEmailEvent
                    {
                        Email = user.Email,
                        UserName = user.Name,
                        UserLastName = user.LastName,
                        LotteryId = lottery.LotteryId,
                        LotteryTitle = lottery.Title,
                        LotteryDescription = lottery.Description,
                        TicketPrice = lottery.TicketPrice,
                        StartDate = lottery.StartDate,
                        EndDate = lottery.EndDate,
                        MaxTickets = lottery.MaxTickets,
                        CampaignId = campaignId,
                        BatchNumber = batchNumber
                    });
                    
                    totalQueued++;
                }
                
                _logger.LogDebug(
                    "Campaign {CampaignId}: Queued batch {BatchNumber} ({Count} emails)",
                    campaignId, batchNumber, batch.Length);
            }

            _logger.LogInformation(
                "Campaign {CampaignId}: Successfully queued {Total} marketing emails for lottery {LotteryId} in {Batches} batches",
                campaignId, totalQueued, lottery.LotteryId, batchNumber);
        }
        catch (RequestTimeoutException ex)
        {
            _logger.LogError(ex, 
                "Campaign {CampaignId}: Timeout waiting for Identity service response for lottery {LotteryId}",
                campaignId, lottery.LotteryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Campaign {CampaignId}: Error processing LotteryCreatedEvent for lottery {LotteryId}",
                campaignId, lottery.LotteryId);
            throw;
        }
    }
}