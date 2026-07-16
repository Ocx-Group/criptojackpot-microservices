using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Identity.Data.Repositories;

public class WishListRepository(IdentityDbContext context) : IWishListRepository
{
    public async Task<IEnumerable<WishListItem>> GetByUserIdAsync(long userId)
        => await context.WishListItems
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<WishListItem?> GetByUserAndLotteryAsync(long userId, Guid lotteryGuid)
        => await context.WishListItems
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LotteryGuid == lotteryGuid);

    public async Task<WishListItem> AddAsync(WishListItem item)
    {
        await context.WishListItems.AddAsync(item);
        await context.SaveChangesAsync();
        return item;
    }

    public async Task RemoveAsync(WishListItem item)
    {
        context.WishListItems.Remove(item);
        await context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(long userId, Guid lotteryGuid)
        => await context.WishListItems
            .AnyAsync(x => x.UserId == userId && x.LotteryGuid == lotteryGuid);
}
