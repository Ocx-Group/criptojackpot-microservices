using CryptoJackpot.Domain.Core.Protos;
using CryptoJackpot.Wallet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Services;

/// <summary>
/// gRPC client wrapper that queries Identity service for user verification.
/// Used for withdrawal verification (get user info, verify TOTP codes).
/// </summary>
public class UserVerificationGrpcClient : IUserVerificationGrpcClient
{
    private readonly UserVerificationGrpcService.UserVerificationGrpcServiceClient _client;
    private readonly ILogger<UserVerificationGrpcClient> _logger;

    public UserVerificationGrpcClient(
        UserVerificationGrpcService.UserVerificationGrpcServiceClient client,
        ILogger<UserVerificationGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<UserInfoResult?> GetUserInfoAsync(Guid userGuid, CancellationToken ct = default)
    {
        _logger.LogDebug("Querying Identity for user info {UserGuid}", userGuid);

        var response = await _client.GetUserInfoAsync(
            new UserInfoRequest { UserGuid = userGuid.ToString() },
            cancellationToken: ct);

        if (!response.Found)
        {
            _logger.LogDebug("Identity reports user not found for {UserGuid}", userGuid);
            return null;
        }

        return new UserInfoResult
        {
            Email = response.Email,
            Name = response.Name,
            LastName = response.LastName,
            TwoFactorEnabled = response.TwoFactorEnabled,
        };
    }

    public async Task<bool> VerifyTotpCodeAsync(Guid userGuid, string code, CancellationToken ct = default)
    {
        _logger.LogDebug("Verifying TOTP code for user {UserGuid}", userGuid);

        var response = await _client.VerifyTotpCodeAsync(
            new VerifyTotpRequest
            {
                UserGuid = userGuid.ToString(),
                Code = code,
            },
            cancellationToken: ct);

        if (!response.IsValid)
        {
            _logger.LogDebug("TOTP verification failed for user {UserGuid}: {Error}",
                userGuid, response.ErrorMessage);
        }

        return response.IsValid;
    }
}
