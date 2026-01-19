using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByGuidAsync(Guid userGuid);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetBySecurityCodeAsync(string securityCode);
    Task<bool> ExistsByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<IEnumerable<User>> GetAllAsync(long? excludeUserId = null);
}
