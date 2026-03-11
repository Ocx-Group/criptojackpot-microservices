using AutoMapper;
using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Handlers.Queries;

public class GetAllTransactionsQueryHandler
    : IRequestHandler<GetAllTransactionsQuery, Result<PagedList<WalletTransactionDto>>>
{
    private readonly IWalletRepository _repository;
    private readonly IMapper _mapper;

    public GetAllTransactionsQueryHandler(IWalletRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<WalletTransactionDto>>> Handle(
        GetAllTransactionsQuery request, CancellationToken cancellationToken)
    {
        var pagedTransactions = await _repository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.Type,
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
