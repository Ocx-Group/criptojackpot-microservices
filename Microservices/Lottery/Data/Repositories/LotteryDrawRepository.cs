using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Lottery.Data.Context;
using CryptoJackpot.Lottery.Domain.Interfaces;
using CryptoJackpot.Lottery.Domain.Models;
using Microsoft.EntityFrameworkCore;
namespace CryptoJackpot.Lottery.Data.Repositories;

public class LotteryDrawRepository : ILotteryDrawRepository
{
    private readonly LotteryDbContext _context;

    public LotteryDrawRepository(LotteryDbContext context)
    {
        _context = context;
    }

    public async Task<LotteryDraw> CreateLotteryAsync(LotteryDraw lotteryDraw)
    {
        var today = DateTime.UtcNow;
        lotteryDraw.CreatedAt = today;
        lotteryDraw.UpdatedAt = today;

        await _context.LotteryDraws.AddAsync(lotteryDraw);
        await _context.SaveChangesAsync();

        return lotteryDraw;
    }

    public async Task<LotteryDraw?> GetLotteryByGuidAsync(Guid lotteryGuid)
        => await _context.LotteryDraws
            .Include(x => x.Prizes)
            .FirstOrDefaultAsync(x => x.LotteryGuid == lotteryGuid);

    public async Task<PagedList<LotteryDraw>> GetAllLotteryDrawsAsync(Pagination pagination)
    {
        var totalItems = await _context.LotteryDraws.CountAsync();

        var lotteries = await _context.LotteryDraws
            .Include(x => x.Prizes)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedList<LotteryDraw>
        {
            Items = lotteries,
            TotalItems = totalItems,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<LotteryDraw> UpdateLotteryDrawAsync(LotteryDraw lotteryDraw)
    {
        var today = DateTime.UtcNow;
        lotteryDraw.UpdatedAt = today;

        _context.LotteryDraws.Update(lotteryDraw);
        await _context.SaveChangesAsync();
        return lotteryDraw;
    }

    public async Task<LotteryDraw> DeleteLotteryDrawAsync(LotteryDraw lotteryDraw)
    {
        var today = DateTime.UtcNow;
        lotteryDraw.UpdatedAt = today;
        lotteryDraw.DeletedAt = today;

        _context.Update(lotteryDraw);
        await _context.SaveChangesAsync();
        return lotteryDraw;
    }
}