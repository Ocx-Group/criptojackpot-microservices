using CryptoJackpot.Lottery.Data.Context;
using CryptoJackpot.Lottery.Domain.Interfaces;
using CryptoJackpot.Lottery.Domain.Models;
using Microsoft.EntityFrameworkCore;
namespace CryptoJackpot.Lottery.Data.Repositories;

public class LotteryNumberRepository : ILotteryNumberRepository
{
    private readonly LotteryDbContext _context;

    public LotteryNumberRepository(LotteryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LotteryNumber>> GetNumbersByLotteryAsync(Guid lotteryId)
        => await _context.LotteryNumbers.Where(x => x.LotteryId == lotteryId).ToListAsync();

    /// <summary>
    /// Obtiene solo los números vendidos (más eficiente para exclusión)
    /// </summary>
    public async Task<HashSet<int>> GetSoldNumbersAsync(Guid lotteryId)
        => (await _context.LotteryNumbers
                .Where(x => x.LotteryId == lotteryId && !x.IsAvailable)
                .Select(x => x.Number)
                .ToListAsync())
            .ToHashSet();

    /// <summary>
    /// Verifica si un número específico está disponible (O(1) en DB)
    /// </summary>
    public async Task<bool> IsNumberAvailableAsync(Guid lotteryId, int number, int series)
        => !await _context.LotteryNumbers
            .AnyAsync(x => x.LotteryId == lotteryId && x.Number == number && x.Series == series);

    /// <summary>
    /// Obtiene N números aleatorios disponibles directamente desde la DB
    /// </summary>
    public async Task<List<int>> GetRandomAvailableNumbersAsync(Guid lotteryId, int count, int maxNumber, int minNumber = 1)
    {
        var soldNumbers = await _context.LotteryNumbers
            .Where(x => x.LotteryId == lotteryId)
            .Select(x => x.Number)
            .ToListAsync();

        var soldSet = soldNumbers.ToHashSet();

        // Generar números disponibles en memoria (más rápido que consultar todos)
        var availableNumbers = Enumerable.Range(minNumber, maxNumber)
            .Where(n => !soldSet.Contains(n))
            .OrderBy(_ => Guid.NewGuid()) // Aleatorio
            .Take(count)
            .ToList();

        return availableNumbers;
    }

    /// <summary>
    /// Agrega múltiples números de lotería
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<LotteryNumber> lotteryNumbers)
    {
        await _context.LotteryNumbers.AddRangeAsync(lotteryNumbers);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Libera los números asociados a un ticket (elimina los registros)
    /// </summary>
    public async Task<bool> ReleaseNumbersByTicketAsync(Guid ticketId)
    {
        var numbers = await _context.LotteryNumbers
            .Where(x => x.TicketId == ticketId)
            .ToListAsync();

        if (!numbers.Any())
            return false;

        _context.LotteryNumbers.RemoveRange(numbers);
        await _context.SaveChangesAsync();
        return true;
    }
}