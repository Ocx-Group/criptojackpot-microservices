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
        try
        {
            _logger.LogInformation("JoinLottery called with lotteryId: {LotteryId}", lotteryId);
            
            var groupName = GetLotteryGroupName(lotteryId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} joined lottery {LotteryId}", Context.ConnectionId, lotteryId);
            
            // Send current available numbers to the newly connected client
            var availableNumbers = await _lotteryNumberService.GetAvailableNumbersAsync(lotteryId);
            _logger.LogInformation("Retrieved {Count} available numbers for lottery {LotteryId}", availableNumbers.Count, lotteryId);
            
            await Clients.Caller.ReceiveAvailableNumbers(lotteryId, availableNumbers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JoinLottery for lottery {LotteryId}: {Message}", lotteryId, ex.Message);
            await Clients.Caller.ReceiveError($"Error joining lottery: {ex.Message}");
        }
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
        try
        {
            _logger.LogInformation(
                "ReserveNumber called - LotteryId: {LotteryId}, Number: {Number}, Quantity: {Quantity}",
                lotteryId, number, quantity);

            var userId = GetUserId();
            _logger.LogInformation("UserId from token: {UserId}", userId);
            
            if (userId == null)
            {
                _logger.LogWarning("ReserveNumber failed: Unauthorized - no userId in token");
                await Clients.Caller.ReceiveError("Unauthorized");
                return;
            }

            _logger.LogInformation("Calling ReserveNumberByQuantityAsync...");
            var result = await _lotteryNumberService.ReserveNumberByQuantityAsync(lotteryId, number, quantity, userId.Value);
            _logger.LogInformation("ReserveNumberByQuantityAsync result - IsSuccess: {IsSuccess}", result.IsSuccess);
            
            if (result.IsSuccess)
            {
                var reservations = result.Value;
                _logger.LogInformation("Reservations count: {Count}", reservations.Count);
                
                var groupName = GetLotteryGroupName(lotteryId);
                
                // Notify all clients in the lottery group about each reserved series
                foreach (var reservation in reservations)
                {
                    _logger.LogInformation(
                        "Notifying group about reservation - Number: {Number}, Series: {Series}",
                        reservation.Number, reservation.Series);
                        
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
                var errorMessage = result.Errors.First().Message;
                _logger.LogWarning("ReserveNumber failed: {Error}", errorMessage);
                await Clients.Caller.ReceiveError(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in ReserveNumber - LotteryId: {LotteryId}, Number: {Number}, Quantity: {Quantity}",
                lotteryId, number, quantity);
            await Clients.Caller.ReceiveError($"Error reserving number: {ex.Message}");
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
        // Try NameIdentifier first (standard .NET claim)
        var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        // Fallback to 'sub' claim (standard JWT claim)
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = Context.User?.FindFirst("sub")?.Value;
        }
        
        _logger.LogDebug("GetUserId - Claims: {Claims}", 
            string.Join(", ", Context.User?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>()));
        
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

