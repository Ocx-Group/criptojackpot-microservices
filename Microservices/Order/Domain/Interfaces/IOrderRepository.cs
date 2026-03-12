using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Order.Domain.Enums;

namespace CryptoJackpot.Order.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Models.Order> CreateAsync(Models.Order order);
    Task<Models.Order?> GetByGuidAsync(Guid orderGuid);
    Task<Models.Order?> GetByGuidWithTrackingAsync(Guid orderGuid);
    Task<Models.Order?> GetByInvoiceIdWithTrackingAsync(string invoiceId);
    Task<IEnumerable<Models.Order>> GetByUserIdAsync(long userId);
    Task<IEnumerable<Models.Order>> GetExpiredPendingOrdersAsync();
    Task<List<Models.Order>> GetExpiredPendingOrdersAsync(DateTime cutoffTime, CancellationToken cancellationToken = default);
    Task<PagedList<Models.Order>> GetAllAsync(int page, int pageSize, OrderStatus? status = null, CancellationToken cancellationToken = default);
    Task<Models.Order> UpdateAsync(Models.Order order);
    Task SaveChangesAsync();
    
    /// <summary>
    /// Gets count of completed orders within a date range.
    /// </summary>
    Task<int> CountCompletedAsync(DateTime? from = null, DateTime? to = null);
    
    /// <summary>
    /// Gets count of all orders (any status) within a date range.
    /// </summary>
    Task<int> CountAllAsync(DateTime? from = null, DateTime? to = null);
    
    /// <summary>
    /// Gets sum of revenue from completed orders within a date range.
    /// </summary>
    Task<decimal> SumRevenueAsync(DateTime? from = null, DateTime? to = null);
}

