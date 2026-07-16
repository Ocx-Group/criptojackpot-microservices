using CryptoJackpot.Domain.Core.Protos;
using CryptoJackpot.Wallet.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Services;

/// <summary>
/// gRPC client wrapper that queries Identity service to resolve referral relationships.
/// Encapsulates the generated gRPC client and provides a clean domain interface.
/// </summary>
public class ReferralGrpcClient : IReferralGrpcClient
{
    private readonly ReferralGrpcService.ReferralGrpcServiceClient _client;
    private readonly ILogger<ReferralGrpcClient> _logger;

    public ReferralGrpcClient(
        ReferralGrpcService.ReferralGrpcServiceClient client,
        ILogger<ReferralGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Guid?> GetReferrerUserGuidAsync(Guid referredUserGuid, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Querying Identity for referrer of user {ReferredGuid}", referredUserGuid);

        var request = new GetReferrerRequest { ReferredUserGuid = referredUserGuid.ToString() };

        var response = await _client.GetReferrerAsync(
            request,
            cancellationToken: cancellationToken);

        if (!response.Found)
        {
            _logger.LogDebug("Identity reports no referrer for user {ReferredGuid}", referredUserGuid);
            return null;
        }

        if (!Guid.TryParse(response.ReferrerUserGuid, out var referrerGuid))
        {
            _logger.LogError(
                "Identity returned an invalid referrer GUID: {ReferrerGuid} for referred user {ReferredGuid}",
                response.ReferrerUserGuid, referredUserGuid);
            return null;
        }

        _logger.LogDebug(
            "Referrer resolved via Identity gRPC: {ReferredGuid} → {ReferrerGuid}",
            referredUserGuid, referrerGuid);

        return referrerGuid;
    }
}

