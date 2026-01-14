using CryptoJackpot.Lottery.Domain.Interfaces;
using CryptoJackpot.Lottery.Domain.Models;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Utilities;

/// <summary>
/// Utility class for batch insertion of lottery numbers.
/// Provides optimized bulk insert operations to improve database performance.
/// </summary>
public class BatchInsertHelper
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILogger _logger;
    private const int DefaultBatchSize = 1000;

    public BatchInsertHelper(
        ILotteryNumberRepository lotteryNumberRepository,
        ILogger logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _logger = logger;
    }

    /// <summary>
    /// Inserts lottery numbers in batches to avoid memory issues
    /// and improve database performance with bulk operations.
    /// </summary>
    /// <param name="lotteryNumbers">Enumerable of lottery numbers to insert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="batchSize">Number of records per batch (default: 1000)</param>
    /// <returns>Total number of records inserted</returns>
    public async Task<int> InsertInBatchesAsync(
        IEnumerable<LotteryNumber> lotteryNumbers,
        CancellationToken cancellationToken,
        int batchSize = DefaultBatchSize)
    {
        var batch = new List<LotteryNumber>(batchSize);
        var batchCount = 0;
        var totalInserted = 0;

        foreach (var lotteryNumber in lotteryNumbers)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Lottery number insertion was cancelled after {TotalInserted} records", totalInserted);
                break;
            }

            batch.Add(lotteryNumber);

            if (batch.Count >= batchSize)
            {
                await _lotteryNumberRepository.AddRangeAsync(batch);
                batchCount++;
                totalInserted += batch.Count;
                _logger.LogDebug("Inserted batch {BatchNumber} ({BatchSize} records)", batchCount, batch.Count);
                batch.Clear();
            }
        }

        // Insert remaining records
        if (batch.Count > 0)
        {
            await _lotteryNumberRepository.AddRangeAsync(batch);
            batchCount++;
            totalInserted += batch.Count;
            _logger.LogDebug("Inserted final batch {BatchNumber} ({BatchSize} records)", batchCount, batch.Count);
        }

        return totalInserted;
    }
}

