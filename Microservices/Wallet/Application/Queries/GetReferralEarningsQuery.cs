using CryptoJackpot.Wallet.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Queries;

public class GetReferralEarningsQuery : IRequest<Result<ReferralEarningsDto>>
{
    public Guid UserGuid { get; set; }
}

