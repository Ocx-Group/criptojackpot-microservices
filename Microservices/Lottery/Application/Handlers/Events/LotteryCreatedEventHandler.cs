using CryptoJackpot.Lottery.Application.Events;
using CryptoJackpot.Lottery.Application.Utilities;
using CryptoJackpot.Lottery.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Events;

/// <summary>
/// Handles the LotteryCreatedEvent to generate all lottery numbers.
/// Generates numbers from MinNumber to MaxNumber for each series.
/// Example: For 10,000 total numbers with range 00-99:
/// - TotalSeries = 100
/// - Creates: Series 01 (00-99), Series 02 (00-99), ..., Series 100 (00-99)
/// </summary>
public class LotteryCreatedEventHandler : INotificationHandler<LotteryCreatedEvent>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILogger<LotteryCreatedEventHandler> _logger;

    public LotteryCreatedEventHandler(
        ILotteryNumberRepository lotteryNumberRepository,
        ILogger<LotteryCreatedEventHandler> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _logger = logger;
    }

    public async Task Handle(LotteryCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Generating lottery numbers for Lottery {LotteryId}. Range: {MinNumber}-{MaxNumber}, Series: {TotalSeries}",
            notification.LotteryId, notification.MinNumber, notification.MaxNumber, notification.TotalSeries);

        try
        {
            var lotteryNumbers = LotteryNumbersGenerator.Generate(
                notification.LotteryId,
                notification.MinNumber,
                notification.MaxNumber,
                notification.TotalSeries);

            var batchHelper = new BatchInsertHelper(_lotteryNumberRepository, _logger);
            var totalInserted = await batchHelper.InsertInBatchesAsync(lotteryNumbers, cancellationToken);

            _logger.LogInformation(
                "Successfully generated {TotalNumbers} lottery numbers for Lottery {LotteryId}",
                totalInserted, notification.LotteryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to generate lottery numbers for Lottery {LotteryId}", 
                notification.LotteryId);
            throw;
        }
    }
}

