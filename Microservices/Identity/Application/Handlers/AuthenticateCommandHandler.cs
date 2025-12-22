using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Events;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers;

public class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, ResultResponse<UserDto?>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventBus _eventBus;

    public AuthenticateCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IEventBus eventBus)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _eventBus = eventBus;
    }

    public async Task<ResultResponse<UserDto?>> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email);

        if (user == null || !_passwordHasher.Verify(user.Password, request.Password))
            return ResultResponse<UserDto?>.Failure(ErrorType.Unauthorized, "Invalid Credentials");

        if (!user.Status)
            return ResultResponse<UserDto?>.Failure(ErrorType.Forbidden, "User Not Verified");

        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            LastName = user.LastName,
            Email = user.Email,
            ImagePath = user.ImagePath,
            Role = new RoleDto
            {
                Id = user.Role.Id,
                Name = user.Role.Name
            },
            Token = _jwtTokenService.GenerateToken(user.Id.ToString())
        };

        // Publish event for other microservices (Notification, Audit, etc.)
        await _eventBus.Publish(new UserLoggedInEvent(user.Id, user.Email, $"{user.Name} {user.LastName}"));

        return ResultResponse<UserDto?>.Ok(userDto);
    }
}