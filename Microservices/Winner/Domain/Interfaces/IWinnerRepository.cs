using CryptoJackpot.Winner.Domain.Models;

namespace CryptoJackpot.Winner.Domain.Interfaces;

public interface IWinnerRepository
{
    Task<LotteryWinner> CreateAsync(LotteryWinner winner);
    Task<LotteryWinner?> GetByGuidAsync(Guid winnerGuid);
    Task<IEnumerable<LotteryWinner>> GetAllAsync();
    Task<LotteryWinner?> GetByLotteryNumberSeriesAsync(Guid lotteryId, int number, int series);
    Task<IEnumerable<LotteryWinner>> GetByLotteryIdAsync(Guid lotteryId);
}
