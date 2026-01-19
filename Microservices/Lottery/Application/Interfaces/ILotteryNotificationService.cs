using CryptoJackpot.Lottery.Application.DTOs;

namespace CryptoJackpot.Lottery.Application.Interfaces;

/// <summary>
/// Service interface for broadcasting lottery updates via SignalR.
/// Used by event consumers to notify connected clients.
/// </summary>
public interface ILotteryNotificationService
{
    /// <summary>
    /// Broadcasts that numbers have been released (available again).
    /// </summary>
    Task NotifyNumbersReleasedAsync(Guid lotteryGuid, List<NumberStatusDto> numbers);

    /// <summary>
    /// Broadcasts that numbers have been sold.
    /// </summary>
    Task NotifyNumbersSoldAsync(Guid lotteryGuid, List<NumberStatusDto> numbers);

    /// <summary>
    /// Broadcasts that a single number has been reserved.
    /// </summary>
    Task NotifyNumberReservedAsync(Guid lotteryGuid, long numberId, Guid numberGuid, int number, int series);
}

