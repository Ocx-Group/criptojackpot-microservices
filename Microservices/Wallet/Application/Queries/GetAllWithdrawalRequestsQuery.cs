using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Domain.Enums;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Queries;

public class GetAllWithdrawalRequestsQuery : IRequest<Result<PagedList<WithdrawalRequestDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public WithdrawalRequestStatus? Status { get; set; }
}
