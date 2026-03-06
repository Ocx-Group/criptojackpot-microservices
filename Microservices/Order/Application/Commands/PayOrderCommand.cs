using CryptoJackpot.Order.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Commands;

public class PayOrderCommand : IRequest<Result<PayOrderResponse>>
{
    public Guid OrderId { get; set; }
    public long UserId { get; set; }
}
