using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByGuidAsync(Guid userGuid);
    Task<User?> GetByGuidWithRecoveryCodesAsync(Guid userGuid);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailCaseInsensitiveAsync(string email);
    Task<User?> GetByGoogleIdAsync(string googleId);
    Task<User?> GetByReferralCodeAsync(string referralCode);
    Task<bool> ExistsByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<IEnumerable<User>> GetAllAsync(long? excludeUserId = null);
    
    /// <summary>
    /// Gets the total count of active users.
    /// </summary>
    Task<int> CountAsync();
    
    /// <summary>
    /// Gets the count of users created within a date range.
    /// </summary>
    Task<int> CountByDateRangeAsync(DateTime from, DateTime to);
}
