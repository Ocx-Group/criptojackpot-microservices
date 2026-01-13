using CryptoJackpot.Lottery.Domain.Models;
namespace CryptoJackpot.Lottery.Domain.Interfaces;

public interface ILotteryNumberRepository
{
    Task<IEnumerable<LotteryNumber>> GetNumbersByLotteryAsync(Guid lotteryId);
    Task<HashSet<int>> GetSoldNumbersAsync(Guid lotteryId);
    Task<bool> IsNumberAvailableAsync(Guid lotteryId, int number, int series);
    Task<List<int>> GetRandomAvailableNumbersAsync(Guid lotteryId, int count, int maxNumber, int minNumber = 1);
    Task AddRangeAsync(IEnumerable<LotteryNumber> lotteryNumbers);
    Task<bool> ReleaseNumbersByTicketAsync(Guid ticketId);
}