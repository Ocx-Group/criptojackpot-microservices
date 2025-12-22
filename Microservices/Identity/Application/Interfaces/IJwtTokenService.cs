namespace CryptoJackpot.Identity.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(string userId);
}

