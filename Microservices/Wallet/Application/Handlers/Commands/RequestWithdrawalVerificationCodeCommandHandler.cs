using System.Security.Cryptography;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Handlers.Commands;

public class RequestWithdrawalVerificationCodeCommandHandler
    : IRequestHandler<RequestWithdrawalVerificationCodeCommand, Result<bool>>
{
    private const int CodeTtlMinutes = 15;
    private const int ResendCooldownSeconds = 60;
    private const string CacheKeyPrefix = "withdrawal-code:";
    private const string CooldownKeyPrefix = "withdrawal-cooldown:";

    private readonly IUserVerificationGrpcClient _userVerificationClient;
    private readonly IDistributedCache _cache;
    private readonly IEventBus _eventBus;
    private readonly ILogger<RequestWithdrawalVerificationCodeCommandHandler> _logger;

    public RequestWithdrawalVerificationCodeCommandHandler(
        IUserVerificationGrpcClient userVerificationClient,
        IDistributedCache cache,
        IEventBus eventBus,
        ILogger<RequestWithdrawalVerificationCodeCommandHandler> logger)
    {
        _userVerificationClient = userVerificationClient;
        _cache = cache;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        RequestWithdrawalVerificationCodeCommand request,
        CancellationToken cancellationToken)
    {
        // Check cooldown to prevent spam
        var cooldownKey = $"{CooldownKeyPrefix}{request.UserGuid}";
        var existingCooldown = await _cache.GetStringAsync(cooldownKey, cancellationToken);
        if (existingCooldown is not null)
        {
            return Result.Fail(new BadRequestError("Please wait before requesting a new code."));
        }

        // Get user info from Identity service
        var userInfo = await _userVerificationClient.GetUserInfoAsync(request.UserGuid, cancellationToken);
        if (userInfo is null)
        {
            return Result.Fail(new NotFoundError("User not found."));
        }

        if (userInfo.TwoFactorEnabled)
        {
            return Result.Fail(new BadRequestError("Two-factor authentication is enabled. Use your authenticator app."));
        }

        // Generate 6-digit code
        var securityCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        // Store hashed code in distributed cache
        var codeHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(securityCode)));

        var cacheKey = $"{CacheKeyPrefix}{request.UserGuid}";
        await _cache.SetStringAsync(cacheKey, codeHash, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CodeTtlMinutes),
        }, cancellationToken);

        // Set cooldown
        await _cache.SetStringAsync(cooldownKey, "1", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ResendCooldownSeconds),
        }, cancellationToken);

        // Publish event for Notification service to send email
        try
        {
            await _eventBus.Publish(new WithdrawalVerificationRequestedEvent
            {
                UserGuid = request.UserGuid,
                Email = userInfo.Email,
                Name = userInfo.Name,
                LastName = userInfo.LastName,
                SecurityCode = securityCode,
                Amount = 0, // Amount not known yet at code request time
            });
            _logger.LogInformation("WithdrawalVerificationRequestedEvent published for user {UserGuid}", request.UserGuid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish WithdrawalVerificationRequestedEvent for user {UserGuid}", request.UserGuid);
            return Result.Fail(new InternalServerError("Failed to send verification code."));
        }

        return Result.Ok(true);
    }
}
