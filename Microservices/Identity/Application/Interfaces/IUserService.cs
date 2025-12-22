using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.DTOs;

namespace CryptoJackpot.Identity.Application.Interfaces;

public interface IUserService
{
    Task<ResultResponse<UserDto?>> CreateUserAsync(CreateUserRequest request);
    Task<ResultResponse<UserDto?>> GetUserByIdAsync(long userId);
    Task<ResultResponse<IEnumerable<UserDto>>> GetAllUsersAsync(long? excludeUserId = null);
    Task<ResultResponse<UserDto?>> UpdateUserAsync(long userId, UpdateUserRequest request);
    Task<ResultResponse<UserDto?>> UpdatePasswordAsync(UpdatePasswordRequest request);
    Task<ResultResponse<string>> RequestPasswordResetAsync(string email);
    Task<ResultResponse<UserDto?>> ResetPasswordWithCodeAsync(ResetPasswordWithCodeRequest request);
    Task<ResultResponse<UserDto?>> GenerateNewSecurityCodeAsync(long userId);
}

