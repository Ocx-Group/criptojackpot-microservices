using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    // Add other methods as needed later
}
