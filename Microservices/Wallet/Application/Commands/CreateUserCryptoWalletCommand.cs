using CryptoJackpot.Wallet.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Commands;

public class CreateUserCryptoWalletCommand : IRequest<Result<UserCryptoWalletDto>>
{
    public Guid UserGuid { get; set; }
    public string Address { get; set; } = null!;
    public string CurrencySymbol { get; set; } = null!;
    public string CurrencyName { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string Label { get; set; } = null!;
}
