using CryptoJackpot.Identity.Data.Context;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Identity.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
