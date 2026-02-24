using CryptoJackpot.Wallet.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Queries;

public class GetUserCryptoWalletsQuery : IRequest<Result<List<UserCryptoWalletDto>>>
{
    public Guid UserGuid { get; set; }
}
