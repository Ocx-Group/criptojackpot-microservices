using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class ResetPasswordWithCodeCommandHandler : IRequestHandler<ResetPasswordWithCodeCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetPasswordWithCodeCommandHandler> _logger;

    public ResetPasswordWithCodeCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<ResetPasswordWithCodeCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ResetPasswordWithCodeCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailCaseInsensitiveAsync(normalizedEmail);

        if (user is null ||
            user.PasswordHash is null ||
            string.IsNullOrWhiteSpace(user.PasswordResetToken) ||
            !user.PasswordResetTokenExpiresAt.HasValue ||
            user.PasswordResetTokenExpiresAt.Value < DateTime.UtcNow ||
            !_passwordHasher.Verify(user.PasswordResetToken, request.SecurityCode))
        {
            return Result.Fail<bool>(new BadRequestError("Invalid or expired security code."));
        }

        if (_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            return Result.Fail<bool>(new BadRequestError("New password must be different from current password."));
        }

        user.PasswordHash = _passwordHasher.Hash(request.Password);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        user.FailedLoginAttempts = 0;
        user.LockoutEndAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id, "Password reset");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset completed for user {UserId}", user.Id);
        return Result.Ok(true);
    }
}
