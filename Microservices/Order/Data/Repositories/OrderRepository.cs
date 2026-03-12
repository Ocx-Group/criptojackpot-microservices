using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Order.Data.Context;
using CryptoJackpot.Order.Domain.Enums;
using CryptoJackpot.Order.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Order.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Models.Order> CreateAsync(Domain.Models.Order order)
    {
        var now = DateTime.UtcNow;
        order.CreatedAt = now;
        order.UpdatedAt = now;

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<Domain.Models.Order?> GetByGuidAsync(Guid orderGuid)
        => await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderGuid == orderGuid);

    public async Task<Domain.Models.Order?> GetByGuidWithTrackingAsync(Guid orderGuid)
        => await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderGuid == orderGuid);

    public async Task<Domain.Models.Order?> GetByInvoiceIdWithTrackingAsync(string invoiceId)
        => await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.InvoiceId == invoiceId);

    public async Task<IEnumerable<Domain.Models.Order>> GetByUserIdAsync(long userId)
        => await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Domain.Models.Order>> GetExpiredPendingOrdersAsync()
        => await _context.Orders
            .Include(o => o.OrderDetails)
            .Where(o => o.Status == OrderStatus.Pending && o.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

    public async Task<List<Domain.Models.Order>> GetExpiredPendingOrdersAsync(
        DateTime cutoffTime, 
        CancellationToken cancellationToken = default)
        => await _context.Orders
            .Include(o => o.OrderDetails)
            .Where(o => o.Status == OrderStatus.Pending && o.ExpiresAt < cutoffTime)
            .ToListAsync(cancellationToken);

    public async Task<Domain.Models.Order> UpdateAsync(Domain.Models.Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<PagedList<Domain.Models.Order>> GetAllAsync(
        int page,
        int pageSize,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Domain.Models.Order>
        {
            Items = items,
            TotalItems = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<int> CountCompletedAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Orders.Where(o => o.Status == OrderStatus.Completed);
        if (from.HasValue) query = query.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(o => o.CreatedAt < to.Value);
        return await query.CountAsync();
    }

    public async Task<int> CountAllAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Orders.AsQueryable();
        if (from.HasValue) query = query.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(o => o.CreatedAt < to.Value);
        return await query.CountAsync();
    }

    public async Task<decimal> SumRevenueAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Orders
            .Where(o => o.Status == OrderStatus.Completed)
            .Include(o => o.OrderDetails)
            .AsQueryable();
        if (from.HasValue) query = query.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(o => o.CreatedAt < to.Value);
        
        return await query
            .SelectMany(o => o.OrderDetails)
            .SumAsync(d => d.Subtotal);
    }
}

