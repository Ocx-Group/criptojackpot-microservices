using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Commands;

public class DeleteUserCryptoWalletCommand : IRequest<Result>
{
    public Guid UserGuid { get; set; }
    public Guid WalletGuid { get; set; }
}
