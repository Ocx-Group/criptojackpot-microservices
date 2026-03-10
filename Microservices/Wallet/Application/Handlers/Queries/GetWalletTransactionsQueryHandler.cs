using AutoMapper;
using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Handlers.Queries;

public class GetWalletTransactionsQueryHandler
    : IRequestHandler<GetWalletTransactionsQuery, Result<PagedList<WalletTransactionDto>>>
{
    private readonly IWalletRepository _repository;
    private readonly IMapper _mapper;

    public GetWalletTransactionsQueryHandler(IWalletRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<WalletTransactionDto>>> Handle(
        GetWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var pagedTransactions = await _repository.GetByUserAsync(
            request.UserGuid,
            request.Type,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtoItems = _mapper.Map<IEnumerable<WalletTransactionDto>>(pagedTransactions.Items);

        var result = new PagedList<WalletTransactionDto>
        {
            Items = dtoItems,
            TotalItems = pagedTransactions.TotalItems,
            PageNumber = pagedTransactions.PageNumber,
            PageSize = pagedTransactions.PageSize,
        };

        return Result.Ok(result);
    }
}
