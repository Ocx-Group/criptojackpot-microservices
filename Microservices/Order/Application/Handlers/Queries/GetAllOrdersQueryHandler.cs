using AutoMapper;
using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Application.Queries;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Handlers.Queries;

public class GetAllOrdersQueryHandler
    : IRequestHandler<GetAllOrdersQuery, Result<PagedList<OrderDto>>>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetAllOrdersQueryHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<OrderDto>>> Handle(
        GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var pagedOrders = await _repository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.Status,
            cancellationToken);

        var dtoItems = _mapper.Map<IEnumerable<OrderDto>>(pagedOrders.Items);

        var result = new PagedList<OrderDto>
        {
            Items = dtoItems,
            TotalItems = pagedOrders.TotalItems,
            PageNumber = pagedOrders.PageNumber,
            PageSize = pagedOrders.PageSize,
        };

        return Result.Ok(result);
    }
}
