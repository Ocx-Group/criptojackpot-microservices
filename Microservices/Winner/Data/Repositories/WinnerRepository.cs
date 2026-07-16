using CryptoJackpot.Winner.Data.Context;
using CryptoJackpot.Winner.Domain.Interfaces;
using CryptoJackpot.Winner.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Winner.Data.Repositories;

public class WinnerRepository : IWinnerRepository
{
    private readonly WinnerDbContext _context;

    public WinnerRepository(WinnerDbContext context)
    {
        _context = context;
    }

    public async Task<LotteryWinner> CreateAsync(LotteryWinner winner)
    {
        var now = DateTime.UtcNow;
        winner.CreatedAt = now;
        winner.UpdatedAt = now;

        await _context.Winners.AddAsync(winner);
        await _context.SaveChangesAsync();

        return winner;
    }

    public async Task<LotteryWinner?> GetByGuidAsync(Guid winnerGuid)
        => await _context.Winners
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WinnerGuid == winnerGuid);

    public async Task<IEnumerable<LotteryWinner>> GetAllAsync()
        => await _context.Winners
            .AsNoTracking()
            .OrderByDescending(w => w.WonAt)
            .ToListAsync();

    public async Task<LotteryWinner?> GetByLotteryNumberSeriesAsync(Guid lotteryId, int number, int series)
        => await _context.Winners
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.LotteryId == lotteryId && w.Number == number && w.Series == series);

    public async Task<IEnumerable<LotteryWinner>> GetByLotteryIdAsync(Guid lotteryId)
        => await _context.Winners
            .AsNoTracking()
            .Where(w => w.LotteryId == lotteryId)
            .OrderByDescending(w => w.WonAt)
            .ToListAsync();
}
