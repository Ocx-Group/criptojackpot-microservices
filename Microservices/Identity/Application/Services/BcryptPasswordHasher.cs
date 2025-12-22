using CryptoJackpot.Identity.Application.Interfaces;

namespace CryptoJackpot.Identity.Application.Services;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string hash, string password)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}

