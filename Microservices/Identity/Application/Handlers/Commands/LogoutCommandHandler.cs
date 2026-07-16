using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IAuthenticationService _authService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IIdentityEventPublisher _eventPublisher;

    public LogoutCommandHandler(
        IAuthenticationService authService,
        IRefreshTokenService refreshTokenService,
        IIdentityEventPublisher eventPublisher)
    {
        _authService = authService;
        _refreshTokenService = refreshTokenService;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            // Resolve user from the refresh token for audit purposes (before revoking)
            var tokenEntity = await _refreshTokenService.ValidateAndGetTokenAsync(request.RefreshToken);

            // Even if token is invalid/expired, we consider logout successful
            // The important thing is to revoke it if it exists
            await _authService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

            // Publish audit event only when user could be identified
            if (tokenEntity?.User is not null)
            {
                await _eventPublisher.PublishUserLoggedOutAsync(
                    tokenEntity.User,
                    request.IpAddress,
                    request.UserAgent);
            }
        }

        return Result.Ok();
    }
}



