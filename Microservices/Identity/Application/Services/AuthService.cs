using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using MediatR;

namespace CryptoJackpot.Identity.Application.Services;

/// <summary>
/// Thin service that delegates to MediatR handlers.
/// Uses MediatR for internal operations (CQRS within the microservice).
/// IEventBus.Publish is used in handlers when other microservices need to be notified.
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
