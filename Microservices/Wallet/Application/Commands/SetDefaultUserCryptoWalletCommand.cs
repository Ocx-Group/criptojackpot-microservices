using CryptoJackpot.Wallet.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Commands;

public class SetDefaultUserCryptoWalletCommand : IRequest<Result<UserCryptoWalletDto>>
{
    public Guid UserGuid { get; set; }
    public Guid WalletGuid { get; set; }
}
