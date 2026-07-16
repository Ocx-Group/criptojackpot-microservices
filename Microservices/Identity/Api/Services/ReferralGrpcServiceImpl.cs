using CryptoJackpot.Domain.Core.Protos;
using CryptoJackpot.Identity.Domain.Interfaces;
using Grpc.Core;

namespace CryptoJackpot.Identity.Api.Services;

/// <summary>
/// gRPC server implementation for referral queries.
/// Allows other microservices (e.g., Wallet) to resolve referrer relationships
/// without maintaining local projections — Identity is the single source of truth.
/// </summary>
public class ReferralGrpcServiceImpl : ReferralGrpcService.ReferralGrpcServiceBase
{
    private readonly IUserReferralRepository _userReferralRepository;
    private readonly ILogger<ReferralGrpcServiceImpl> _logger;

    public ReferralGrpcServiceImpl(
        IUserReferralRepository userReferralRepository,
        ILogger<ReferralGrpcServiceImpl> logger)
    {
        _userReferralRepository = userReferralRepository;
        _logger = logger;
    }

    public override async Task<GetReferrerResponse> GetReferrer(
        GetReferrerRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.ReferredUserGuid, out var referredGuid))
        {
            _logger.LogWarning("Invalid GUID received in GetReferrer: {Guid}", request.ReferredUserGuid);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid referred_user_guid format"));
        }

        var referrerGuid = await _userReferralRepository
            .GetReferrerGuidByReferredGuidAsync(referredGuid, context.CancellationToken);

        if (referrerGuid is null)
        {
            _logger.LogDebug("No referrer found for user {ReferredGuid}", referredGuid);
            return new GetReferrerResponse { Found = false, ReferrerUserGuid = string.Empty };
        }

        _logger.LogDebug(
            "Referrer resolved: {ReferredGuid} → {ReferrerGuid}",
            referredGuid, referrerGuid.Value);

        return new GetReferrerResponse
        {
            Found = true,
            ReferrerUserGuid = referrerGuid.Value.ToString()
        };
    }
}

