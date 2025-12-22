using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, ResultResponse<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<RequestPasswordResetCommandHandler> _logger;

    public RequestPasswordResetCommandHandler(
        IUserRepository userRepository,
        IEventBus eventBus,
        ILogger<RequestPasswordResetCommandHandler> logger)
    {
        _userRepository = userRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<ResultResponse<string>> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            return ResultResponse<string>.Failure(ErrorType.NotFound, "User not found");

        var securityCode = new Random().Next(100000, 999999).ToString();
        user.SecurityCode = securityCode;
        user.PasswordResetCodeExpiration = DateTime.UtcNow.AddMinutes(15);

        await _userRepository.UpdateAsync(user);

        try
        {
            await _eventBus.Publish(new PasswordResetRequestedEvent
            {
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                SecurityCode = securityCode
            });
            _logger.LogInformation("PasswordResetRequestedEvent published for {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish PasswordResetRequestedEvent for {Email}", user.Email);
        }

        return ResultResponse<string>.Ok("Password reset email sent");
    }
}

