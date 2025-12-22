using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Extensions;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class ResetPasswordWithCodeCommandHandler : IRequestHandler<ResetPasswordWithCodeCommand, ResultResponse<UserDto?>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordWithCodeCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ResultResponse<UserDto?>> Handle(ResetPasswordWithCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            return ResultResponse<UserDto?>.Failure(ErrorType.NotFound, "User not found");

        if (string.IsNullOrEmpty(user.SecurityCode) ||
            user.SecurityCode != request.SecurityCode ||
            user.PasswordResetCodeExpiration == null ||
            user.PasswordResetCodeExpiration < DateTime.UtcNow)
        {
            return ResultResponse<UserDto?>.Failure(ErrorType.BadRequest, "Invalid or expired security code");
        }

        if (request.NewPassword != request.ConfirmPassword)
            return ResultResponse<UserDto?>.Failure(ErrorType.BadRequest, "Passwords do not match");

        user.Password = _passwordHasher.Hash(request.NewPassword);
        user.SecurityCode = null;
        user.PasswordResetCodeExpiration = null;

        var updatedUser = await _userRepository.UpdateAsync(user);
        return ResultResponse<UserDto?>.Ok(updatedUser.ToDto());
    }
}

