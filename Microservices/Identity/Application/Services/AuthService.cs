using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using MediatR;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Thin service that delegates to MediatR handlers.
/// This service can be removed if the controller injects IMediator directly.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IMediator _mediator;

    public AuthService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<ResultResponse<UserDto?>> AuthenticateAsync(AuthenticateRequest request)
    {
        var command = new AuthenticateCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        return await _mediator.Send(command);
    }
}
