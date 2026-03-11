using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class AddToWishListCommand : IRequest<Result<WishListItemDto>>
{
    public Guid UserGuid { get; set; }
    public long UserId { get; set; }
    public Guid LotteryGuid { get; set; }
}
