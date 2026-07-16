using CryptoJackpot.Wallet.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Commands;

public class AdminCreditCommand : IRequest<Result<WalletTransactionDto>>
{
    public Guid UserGuid { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
