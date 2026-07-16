using AutoMapper;
using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Handlers.Queries;

public class GetAllWithdrawalRequestsQueryHandler
    : IRequestHandler<GetAllWithdrawalRequestsQuery, Result<PagedList<WithdrawalRequestDto>>>
{
    private readonly IWithdrawalRequestRepository _repository;
    private readonly IMapper _mapper;

    public GetAllWithdrawalRequestsQueryHandler(IWithdrawalRequestRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<WithdrawalRequestDto>>> Handle(
        GetAllWithdrawalRequestsQuery request, CancellationToken cancellationToken)
    {
        var pagedRequests = await _repository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.Status,
            cancellationToken);

        var dtoItems = _mapper.Map<IEnumerable<WithdrawalRequestDto>>(pagedRequests.Items);

        var result = new PagedList<WithdrawalRequestDto>
        {
            Items = dtoItems,
            TotalItems = pagedRequests.TotalItems,
            PageNumber = pagedRequests.PageNumber,
            PageSize = pagedRequests.PageSize,
        };

        return Result.Ok(result);
    }
}
