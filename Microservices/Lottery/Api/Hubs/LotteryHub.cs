using CryptoJackpot.Lottery.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CryptoJackpot.Lottery.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time lottery number updates.
/// Clients connect to this hub to receive instant notifications when numbers are reserved/released/sold.
/// </summary>
[Authorize]
public class LotteryHub : Hub<ILotteryHubClient>
{
    private readonly ILotteryNumberService _lotteryNumberService;
    private readonly ILogger<LotteryHub> _logger;

    public LotteryHub(
        ILotteryNumberService lotteryNumberService,
        ILogger<LotteryHub> logger)
    {
        _lotteryNumberService = lotteryNumberService;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public async override Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected to LotteryHub", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected from LotteryHub", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a lottery room to receive updates for a specific lottery.
    /// </summary>
    /// <param name="lotteryId">The lottery ID to join</param>
    public async Task JoinLottery(Guid lotteryId)
    {
        var groupName = GetLotteryGroupName(lotteryId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined lottery {LotteryId}", Context.ConnectionId, lotteryId);
        
        // Send current available numbers to the newly connected client
        var availableNumbers = await _lotteryNumberService.GetAvailableNumbersAsync(lotteryId);
        await Clients.Caller.ReceiveAvailableNumbers(lotteryId, availableNumbers);
    }

    /// <summary>
    /// Leave a lottery room.
    /// </summary>
    /// <param name="lotteryId">The lottery ID to leave</param>
    public async Task LeaveLottery(Guid lotteryId)
    {
        var groupName = GetLotteryGroupName(lotteryId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left lottery {LotteryId}", Context.ConnectionId, lotteryId);
    }

    /// <summary>
    /// Reserve N series of a number for the current user.
    /// The system automatically assigns the next available series in order.
    /// Example: User requests number 10 with quantity 2 → System assigns Series 1 and Series 2 (if available)
    /// </summary>
    /// <param name="lotteryId">The lottery ID</param>
    /// <param name="number">The number to reserve (e.g., 10)</param>
    /// <param name="quantity">How many series to reserve (default: 1)</param>
    public async Task ReserveNumber(Guid lotteryId, int number, int quantity = 1)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            await Clients.Caller.ReceiveError("Unauthorized");
            return;
        }

        var result = await _lotteryNumberService.ReserveNumberByQuantityAsync(lotteryId, number, quantity, userId.Value);
        
        if (result.IsSuccess)
        {
            var reservations = result.Value;
            var groupName = GetLotteryGroupName(lotteryId);
            
            // Notify all clients in the lottery group about each reserved series
            foreach (var reservation in reservations)
            {
                await Clients.Group(groupName).NumberReserved(
                    lotteryId, 
                    reservation.NumberId, 
                    reservation.Number, 
                    reservation.Series);
            }
            
            // Confirm all reservations to the caller
            await Clients.Caller.ReservationsConfirmed(reservations);
            
            _logger.LogInformation(
                "User {UserId} reserved {Count} series of number {Number} in lottery {LotteryId}. Series: [{Series}]",
                userId, 
                reservations.Count, 
                number, 
                lotteryId,
                string.Join(", ", reservations.Select(r => r.Series)));
        }
        else
        {
            await Clients.Caller.ReceiveError(result.Errors.First().Message);
        }
    }

    /// <summary>
    /// Get the group name for a lottery.
    /// </summary>
    private static string GetLotteryGroupName(Guid lotteryId) => $"lottery-{lotteryId}";

    /// <summary>
    /// Get the current user ID from the connection context.
    /// </summary>
    private long? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

