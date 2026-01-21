using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Identity.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Consumers;

/// <summary>
/// Consumer that handles GetUsersForMarketingRequestEvent from Notification service.
/// Publishes GetUsersForMarketingResponseEvent with the list of users for marketing campaigns.
/// This implements the Saga pattern for async Request/Response via Kafka.
/// </summary>
public class GetUsersForMarketingConsumer : IConsumer<GetUsersForMarketingRequestEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<GetUsersForMarketingConsumer> _logger;

    public GetUsersForMarketingConsumer(
        IUserRepository userRepository,
        IEventBus eventBus,
        ILogger<GetUsersForMarketingConsumer> logger)
    {
        _userRepository = userRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetUsersForMarketingRequestEvent> context)
    {
        var request = context.Message;
        
        _logger.LogInformation(
            "Received GetUsersForMarketingRequestEvent. CorrelationId: {CorrelationId}, LotteryId: {LotteryId}",
            request.CorrelationId, request.LotteryId);

        try
        {
            var users = await _userRepository.GetAllAsync();
            
            // Filter based on request options
            var filteredUsers = users.Where(u => 
                (!request.OnlyActiveUsers || u.Status));

            var userInfoList = filteredUsers.Select(u => new MarketingUserInfo
            {
                UserGuid = u.UserGuid,
                Email = u.Email,
                Name = u.Name,
                LastName = u.LastName
            }).ToList();

            _logger.LogInformation(
                "Publishing GetUsersForMarketingResponseEvent with {Count} users for CorrelationId: {CorrelationId}",
                userInfoList.Count, request.CorrelationId);

            // Publish response event with users and lottery info
            await _eventBus.Publish(new GetUsersForMarketingResponseEvent
            {
                CorrelationId = request.CorrelationId,
                Users = userInfoList,
                Success = true,
                // Pass through lottery information
                LotteryId = request.LotteryId,
                LotteryTitle = request.LotteryTitle,
                LotteryDescription = request.LotteryDescription,
                TicketPrice = request.TicketPrice,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                MaxTickets = request.MaxTickets
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error processing GetUsersForMarketingRequestEvent for CorrelationId: {CorrelationId}", 
                request.CorrelationId);
            
            // Publish error response
            await _eventBus.Publish(new GetUsersForMarketingResponseEvent
            {
                CorrelationId = request.CorrelationId,
                Users = [],
                Success = false,
                ErrorMessage = ex.Message,
                LotteryId = request.LotteryId,
                LotteryTitle = request.LotteryTitle,
                LotteryDescription = request.LotteryDescription,
                TicketPrice = request.TicketPrice,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                MaxTickets = request.MaxTickets
            });
        }
    }
}
