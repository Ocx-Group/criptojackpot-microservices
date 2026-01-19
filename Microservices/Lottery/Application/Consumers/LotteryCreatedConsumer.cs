using CryptoJackpot.Domain.Core.IntegrationEvents.Lottery;
using CryptoJackpot.Lottery.Application.Utilities;
using CryptoJackpot.Lottery.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Consumers;

/// <summary>
/// Consumes LotteryCreatedEvent from the message bus to generate lottery numbers asynchronously.
/// This allows the API to respond immediately while number generation happens in the background.
/// </summary>
public class LotteryCreatedConsumer : IConsumer<LotteryCreatedEvent>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILogger<LotteryCreatedConsumer> _logger;

    public LotteryCreatedConsumer(
        ILotteryNumberRepository lotteryNumberRepository,
        ILogger<LotteryCreatedConsumer> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LotteryCreatedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Received LotteryCreatedEvent for Lottery {LotteryId}. Range: {MinNumber}-{MaxNumber}, Series: {TotalSeries}",
            message.LotteryId, message.MinNumber, message.MaxNumber, message.TotalSeries);

        try
        {
            var lotteryNumbers = LotteryNumbersGenerator.Generate(
                message.LotteryDbId,
                message.MinNumber,
                message.MaxNumber,
                message.TotalSeries);

            var batchHelper = new BatchInsertHelper(_lotteryNumberRepository, _logger);
            var totalInserted = await batchHelper.InsertInBatchesAsync(lotteryNumbers, context.CancellationToken);

            _logger.LogInformation(
                "Successfully generated {TotalNumbers} lottery numbers for Lottery {LotteryId}",
                totalInserted, message.LotteryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to generate lottery numbers for Lottery {LotteryId}", 
                message.LotteryId);
            throw; // Re-throw to trigger retry policy
        }
    }
}

