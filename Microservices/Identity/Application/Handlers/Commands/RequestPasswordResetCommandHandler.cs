using System.Security.Cryptography;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Result<bool>>
{
    private const int CodeTtlMinutes = 15;
    private const int ResendCooldownSeconds = 60;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly ILogger<RequestPasswordResetCommandHandler> _logger;

    public RequestPasswordResetCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IIdentityEventPublisher eventPublisher,
        ILogger<RequestPasswordResetCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailCaseInsensitiveAsync(normalizedEmail);

        // Always return success to prevent user enumeration.
        if (user is null || user.PasswordHash is null)
        {
            return Result.Ok(true);
        }

        var now = DateTime.UtcNow;
        if (user.PasswordResetTokenExpiresAt.HasValue &&
            user.PasswordResetTokenExpiresAt.Value > now.AddMinutes(CodeTtlMinutes).AddSeconds(-ResendCooldownSeconds))
        {
            _logger.LogInformation("Password reset request throttled for user {UserId}", user.Id);
            return Result.Ok(true);
        }

        var securityCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        user.PasswordResetToken = _passwordHasher.Hash(securityCode);
        user.PasswordResetTokenExpiresAt = now.AddMinutes(CodeTtlMinutes);
        user.UpdatedAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventPublisher.PublishPasswordResetRequestedAsync(user, securityCode);

        return Result.Ok(true);
    }
}
