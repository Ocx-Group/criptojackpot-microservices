using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Handlers.Queries;

public class GetWalletBalanceQueryHandler
    : IRequestHandler<GetWalletBalanceQuery, Result<WalletBalanceDto>>
{
    private readonly IWalletBalanceRepository _balanceRepository;

    public GetWalletBalanceQueryHandler(IWalletBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository;
    }

    public async Task<Result<WalletBalanceDto>> Handle(
        GetWalletBalanceQuery request, CancellationToken cancellationToken)
    {
        var balance = await _balanceRepository.GetByUserAsync(request.UserGuid, cancellationToken);

        var dto = new WalletBalanceDto
        {
            Balance = balance?.Balance ?? 0,
            TotalEarned = balance?.TotalEarned ?? 0,
            TotalWithdrawn = balance?.TotalWithdrawn ?? 0,
            TotalPurchased = balance?.TotalPurchased ?? 0,
        };

        return Result.Ok(dto);
    }
}
