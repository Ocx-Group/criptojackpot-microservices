using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Models;

namespace CryptoJackpot.Wallet.Domain.Interfaces;

public interface IWithdrawalRequestRepository
{
    Task<WithdrawalRequest?> GetByGuidAsync(Guid requestGuid, CancellationToken ct = default);
    Task<WithdrawalRequest?> GetByGuidAndUserAsync(Guid requestGuid, Guid userGuid, CancellationToken ct = default);
    Task<List<WithdrawalRequest>> GetByUserAsync(Guid userGuid, WithdrawalRequestStatus? status = null, CancellationToken ct = default);
    Task<bool> HasPendingRequestAsync(Guid userGuid, CancellationToken ct = default);
    Task<PagedList<WithdrawalRequest>> GetAllAsync(int page, int pageSize, WithdrawalRequestStatus? status = null, CancellationToken ct = default);
    Task<WithdrawalRequest> AddAsync(WithdrawalRequest request, CancellationToken ct = default);
    void Update(WithdrawalRequest request);
}
