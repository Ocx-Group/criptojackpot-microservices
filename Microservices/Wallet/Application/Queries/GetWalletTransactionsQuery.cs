using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Domain.Enums;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Queries;

public class GetWalletTransactionsQuery : IRequest<Result<PagedList<WalletTransactionDto>>>
{
    public Guid UserGuid { get; set; }
    public WalletTransactionType? Type { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
