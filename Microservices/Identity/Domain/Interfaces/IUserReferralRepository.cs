using CryptoJackpot.Identity.Domain.Models;

namespace CryptoJackpot.Identity.Domain.Interfaces
{
    public interface IUserReferralRepository
    {
        Task<UserReferral?> CheckIfUserIsReferred(long userId);
        Task<UserReferral> CreateUserReferralAsync(UserReferral userReferral);
        Task<IEnumerable<UserReferral>> GetAllReferralsByUserId(long userId);
        Task<IEnumerable<UserReferralWithStats>> GetReferralStatsAsync(long userId);

        /// <summary>
        /// Resolves the referrer's UserGuid for a given referred user's UserGuid.
        /// Used by the gRPC service so other microservices can query the single source of truth.
        /// </summary>
        Task<Guid?> GetReferrerGuidByReferredGuidAsync(Guid referredUserGuid, CancellationToken cancellationToken = default);
    }
}
