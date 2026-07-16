using CryptoJackpot.Wallet.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Queries;

public class GetWalletBalanceQuery : IRequest<Result<WalletBalanceDto>>
{
    public Guid UserGuid { get; set; }
}
