using CryptoJackpot.Domain.Core.Protos;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using Grpc.Core;

namespace CryptoJackpot.Identity.Api.Services;

/// <summary>
/// gRPC server implementation for user verification queries.
/// Allows other microservices (e.g., Wallet) to verify user identity
/// for sensitive operations like withdrawals.
/// </summary>
public class UserVerificationGrpcServiceImpl : UserVerificationGrpcService.UserVerificationGrpcServiceBase
{
    private readonly IUserRepository _userRepository;
    private readonly ICodeVerificationService _codeVerificationService;
    private readonly ILogger<UserVerificationGrpcServiceImpl> _logger;

    public UserVerificationGrpcServiceImpl(
        IUserRepository userRepository,
        ICodeVerificationService codeVerificationService,
        ILogger<UserVerificationGrpcServiceImpl> logger)
    {
        _userRepository = userRepository;
        _codeVerificationService = codeVerificationService;
        _logger = logger;
    }

    public override async Task<UserInfoResponse> GetUserInfo(
        UserInfoRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserGuid, out var userGuid))
        {
            _logger.LogWarning("Invalid GUID received in GetUserInfo: {Guid}", request.UserGuid);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user_guid format"));
        }

        var user = await _userRepository.GetByGuidAsync(userGuid);

        if (user is null)
        {
            _logger.LogDebug("User not found for GUID {UserGuid}", userGuid);
            return new UserInfoResponse { Found = false };
        }

        return new UserInfoResponse
        {
            Found = true,
            Email = user.Email,
            Name = user.Name,
            LastName = user.LastName,
            TwoFactorEnabled = user.TwoFactorEnabled,
        };
    }

    public override async Task<VerifyTotpResponse> VerifyTotpCode(
        VerifyTotpRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserGuid, out var userGuid))
        {
            _logger.LogWarning("Invalid GUID received in VerifyTotpCode: {Guid}", request.UserGuid);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user_guid format"));
        }

        var user = await _userRepository.GetByGuidAsync(userGuid);

        if (user is null)
        {
            return new VerifyTotpResponse
            {
                IsValid = false,
                ErrorMessage = "User not found."
            };
        }

        if (!user.TwoFactorEnabled)
        {
            return new VerifyTotpResponse
            {
                IsValid = false,
                ErrorMessage = "Two-factor authentication is not enabled."
            };
        }

        var result = _codeVerificationService.VerifyCode(user, request.Code, null);

        if (result.IsFailed)
        {
            _logger.LogDebug("TOTP verification failed for user {UserGuid}", userGuid);
            return new VerifyTotpResponse
            {
                IsValid = false,
                ErrorMessage = result.Errors.FirstOrDefault()?.Message ?? "Invalid code."
            };
        }

        _logger.LogInformation("TOTP verification successful for user {UserGuid}", userGuid);
        return new VerifyTotpResponse { IsValid = true, ErrorMessage = string.Empty };
    }
}
