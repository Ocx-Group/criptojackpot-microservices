using CryptoJackpot.Lottery.Domain.Models;
namespace CryptoJackpot.Lottery.Domain.Interfaces;

public interface ILotteryNumberRepository
{
    Task<IEnumerable<LotteryNumber>> GetNumbersByLotteryAsync(long lotteryId);
    Task<HashSet<int>> GetSoldNumbersAsync(long lotteryId);
    Task<bool> IsNumberAvailableAsync(long lotteryId, int number, int series);
    Task<List<int>> GetAlreadyReservedNumbersAsync(long lotteryId, int series, IEnumerable<int> numbers);
    Task<List<(int Number, int Series)>> GetRandomAvailableNumbersWithSeriesAsync(
        long lotteryId, int count, int maxNumber, int totalSeries, int minNumber = 1);
    Task<List<int>> GetRandomAvailableNumbersAsync(long lotteryId, int count, int maxNumber, int minNumber = 1);
    Task AddRangeAsync(IEnumerable<LotteryNumber> lotteryNumbers);
    Task<bool> ReleaseNumbersByTicketAsync(Guid ticketId);
    
    // Order integration methods
    Task<bool> ReserveNumbersAsync(List<long> numberIds, Guid orderId);
    Task<bool> ConfirmNumbersSoldAsync(List<long> numberIds, Guid ticketId);
    Task<bool> ReleaseNumbersByOrderAsync(Guid orderId);
    Task<List<LotteryNumber>> GetByIdsAsync(List<long> numberIds);
    
    // Integration events methods (use GUIDs for cross-microservice communication)
    Task<List<LotteryNumber>> GetByGuidsAsync(List<Guid> lotteryNumberGuids);
    Task<bool> ConfirmNumbersSoldByGuidsAsync(List<Guid> lotteryNumberGuids, Guid ticketId);
    Task<bool> ReserveNumbersByGuidsAsync(List<Guid> lotteryNumberGuids, Guid orderId);
    
    // SignalR/Real-time methods
    Task<LotteryNumber?> FindAvailableNumberAsync(long lotteryId, int number, int? series = null);
    Task<List<LotteryNumber>> FindAvailableNumbersAsync(long lotteryId, int series, IEnumerable<int> numbers);
    Task<LotteryNumber> UpdateAsync(LotteryNumber lotteryNumber);
    Task UpdateRangeAsync(IEnumerable<LotteryNumber> lotteryNumbers);
    
    /// <summary>
    /// Gets the next N available series for a specific number, ordered by series ASC.
    /// Used for automatic series assignment when user selects a number.
    /// </summary>
    Task<List<LotteryNumber>> GetNextAvailableSeriesAsync(long lotteryId, int number, int quantity);
    
    /// <summary>
    /// Gets available series count for a specific number.
    /// </summary>
    Task<int> GetAvailableSeriesCountAsync(long lotteryId, int number);
}