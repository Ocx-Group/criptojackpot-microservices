using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Requests;
using CryptoJackpot.Domain.Core.Responses;

namespace CryptoJackpot.Identity.Application.Interfaces;

public interface IAuthService
{
    Task<ResultResponse<UserDto?>> AuthenticateAsync(AuthenticateRequest request);
}