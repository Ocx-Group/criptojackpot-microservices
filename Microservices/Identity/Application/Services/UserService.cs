using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Application.Requests;
using MediatR;

namespace CryptoJackpot.Identity.Application.Services;

public class UserService : IUserService
{
    private readonly IMediator _mediator;

    public UserService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<ResultResponse<UserDto?>> CreateUserAsync(CreateUserRequest request)
    {
        var command = new CreateUserCommand
        {
            Email = request.Email,
            Password = request.Password,
            Name = request.Name,
            LastName = request.LastName,
            Phone = request.Phone,
            CountryId = request.CountryId,
            ReferralCode = request.ReferralCode
        };

        return await _mediator.Send(command);
    }

    public async Task<ResultResponse<UserDto?>> GetUserByIdAsync(long userId)
    {
        return await _mediator.Send(new GetUserByIdQuery { UserId = userId });
    }

    public async Task<ResultResponse<IEnumerable<UserDto>>> GetAllUsersAsync(long? excludeUserId = null)
    {
        return await _mediator.Send(new GetAllUsersQuery { ExcludeUserId = excludeUserId });
    }

    public async Task<ResultResponse<UserDto?>> UpdateUserAsync(long userId, UpdateUserRequest request)
    {
        return await _mediator.Send(new UpdateUserCommand
        {
            UserId = userId,
            Name = request.Name,
            LastName = request.LastName,
            Phone = request.Phone,
            Password = request.Password
        });
    }

    public async Task<ResultResponse<UserDto?>> UpdatePasswordAsync(UpdatePasswordRequest request)
    {
        return await _mediator.Send(new UpdatePasswordCommand
        {
            UserId = request.UserId,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        });
    }

    public async Task<ResultResponse<string>> RequestPasswordResetAsync(string email)
    {
        return await _mediator.Send(new RequestPasswordResetCommand { Email = email });
    }

    public async Task<ResultResponse<UserDto?>> ResetPasswordWithCodeAsync(ResetPasswordWithCodeRequest request)
    {
        return await _mediator.Send(new ResetPasswordWithCodeCommand
        {
            Email = request.Email,
            SecurityCode = request.SecurityCode,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword
        });
    }

    public async Task<ResultResponse<UserDto?>> GenerateNewSecurityCodeAsync(long userId)
    {
        return await _mediator.Send(new GenerateNewSecurityCodeCommand { UserId = userId });
    }
}

