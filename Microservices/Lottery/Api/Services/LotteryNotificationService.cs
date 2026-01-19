using CryptoJackpot.Lottery.Api.Hubs;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CryptoJackpot.Lottery.Api.Services;

/// <summary>
/// Service for broadcasting lottery updates via SignalR.
/// Implemented in the API layer because it needs access to the concrete Hub type.
/// </summary>
public class LotteryNotificationService : ILotteryNotificationService 
{
    private readonly IHubContext<LotteryHub, ILotteryHubClient> _hubContext;
    private readonly ILogger<LotteryNotificationService> _logger;

    public LotteryNotificationService(
        IHubContext<LotteryHub, ILotteryHubClient> hubContext,
        ILogger<LotteryNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyNumbersReleasedAsync(Guid lotteryGuid, List<NumberStatusDto> numbers)
    {
        var groupName = GetLotteryGroupName(lotteryGuid);
        
        await _hubContext.Clients.Group(groupName).NumbersReleased(lotteryGuid, numbers);
        
        _logger.LogInformation(
            "Broadcasted {Count} numbers released for lottery {LotteryId}",
            numbers.Count, lotteryGuid);
    }

    public async Task NotifyNumbersSoldAsync(Guid lotteryGuid, List<NumberStatusDto> numbers)
    {
        var groupName = GetLotteryGroupName(lotteryGuid);
        
        await _hubContext.Clients.Group(groupName).NumbersSold(lotteryGuid, numbers);
        
        _logger.LogInformation(
            "Broadcasted {Count} numbers sold for lottery {LotteryId}",
            numbers.Count, lotteryGuid);
    }

    public async Task NotifyNumberReservedAsync(Guid lotteryGuid, long numberId, Guid numberGuid, int number, int series)
    {
        var groupName = GetLotteryGroupName(lotteryGuid);
        
        await _hubContext.Clients.Group(groupName).NumberReserved(lotteryGuid, numberId, numberGuid, number, series);
        
        _logger.LogInformation(
            "Broadcasted number {Number} series {Series} reserved for lottery {LotteryId}",
            number, series, lotteryGuid);
    }

    private static string GetLotteryGroupName(Guid lotteryId) => $"lottery-{lotteryId}";
}

