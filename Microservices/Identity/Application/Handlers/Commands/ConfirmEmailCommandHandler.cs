using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(
        IUserRepository userRepository,
        ILogger<ConfirmEmailCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Fail<string>(new BadRequestError("Invalid confirmation token"));

        var user = await _userRepository.GetByEmailVerificationTokenAsync(request.Token);

        if (user == null)
            return Result.Fail<string>(new NotFoundError("Invalid or expired confirmation token"));

        if (user.EmailVerified)
            return Result.Ok("Email already confirmed");

        if (user.EmailVerificationTokenExpiresAt.HasValue &&
            user.EmailVerificationTokenExpiresAt.Value < DateTime.UtcNow)
            return Result.Fail<string>(new BadRequestError("Confirmation token has expired"));

        user.EmailVerified = true;
        user.Status = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Email confirmed for user {UserId}", user.Id);

        return Result.Ok("Email confirmed successfully");
    }
}
