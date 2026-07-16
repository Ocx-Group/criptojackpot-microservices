using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Domain.Enums;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Queries;

public class GetAllTransactionsQuery : IRequest<Result<PagedList<WalletTransactionDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public WalletTransactionType? Type { get; set; }
}
