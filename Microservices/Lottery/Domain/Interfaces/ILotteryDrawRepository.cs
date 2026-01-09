using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Lottery.Domain.Models;
namespace CryptoJackpot.Lottery.Domain.Interfaces;

public interface ILotteryDrawRepository
{
    Task<LotteryDraw> CreateLotteryAsync(LotteryDraw lotteryDraw);
    Task<LotteryDraw?> GetLotteryByIdAsync(Guid lotteryDrawId);
    Task<PagedList<LotteryDraw>> GetAllLotteryDrawsAsync(Pagination pagination);
    Task<LotteryDraw> UpdateLotteryDrawAsync(LotteryDraw lotteryDraw);
    Task<LotteryDraw> DeleteLotteryDrawAsync(LotteryDraw lotteryDraw);
}