using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Handlers.Queries;

public class GetReferralEarningsQueryHandler : IRequestHandler<GetReferralEarningsQuery, Result<ReferralEarningsDto>>
{
    private readonly IWalletRepository _repository;

    public GetReferralEarningsQueryHandler(IWalletRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ReferralEarningsDto>> Handle(GetReferralEarningsQuery request, CancellationToken cancellationToken)
    {
        var (totalEarnings, lastMonthEarnings) = await _repository.GetReferralEarningsAsync(
            request.UserGuid, cancellationToken);

        var dto = new ReferralEarningsDto
        {
            TotalEarnings = totalEarnings,
            LastMonthEarnings = lastMonthEarnings
        };

        return Result.Ok(dto);
    }
}

