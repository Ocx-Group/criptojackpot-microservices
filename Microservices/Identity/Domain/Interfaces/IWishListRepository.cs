using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Domain.Interfaces;

public interface IWishListRepository
{
    Task<IEnumerable<WishListItem>> GetByUserIdAsync(long userId);
    Task<WishListItem?> GetByUserAndLotteryAsync(long userId, Guid lotteryGuid);
    Task<WishListItem> AddAsync(WishListItem item);
    Task RemoveAsync(WishListItem item);
    Task<bool> ExistsAsync(long userId, Guid lotteryGuid);
}
