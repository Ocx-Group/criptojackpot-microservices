using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Extensions;
using CryptoJackpot.Identity.Domain.Interfaces;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class GenerateNewSecurityCodeCommandHandler : IRequestHandler<GenerateNewSecurityCodeCommand, ResultResponse<UserDto?>>
{
    private readonly IUserRepository _userRepository;

    public GenerateNewSecurityCodeCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ResultResponse<UserDto?>> Handle(GenerateNewSecurityCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
            return ResultResponse<UserDto?>.Failure(ErrorType.NotFound, "User not found");

        user.SecurityCode = Guid.NewGuid().ToString();
        var updatedUser = await _userRepository.UpdateAsync(user);

        return ResultResponse<UserDto?>.Ok(updatedUser.ToDto());
    }
}

