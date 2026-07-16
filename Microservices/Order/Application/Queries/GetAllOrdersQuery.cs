using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Domain.Enums;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Queries;

public class GetAllOrdersQuery : IRequest<Result<PagedList<OrderDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public OrderStatus? Status { get; set; }
}
