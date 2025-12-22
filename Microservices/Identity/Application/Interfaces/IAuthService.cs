using CryptoJackpot.Identity.Application.DTOs;

namespace CryptoJackpot.Identity.Application.Interfaces;

public interface IAuthService
{
    Task<ResultResponse<UserDto?>> AuthenticateAsync(AuthenticateRequest request);
}
