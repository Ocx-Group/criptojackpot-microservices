using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Queries;

public class GetUserWishListQuery : IRequest<Result<List<WishListItemDto>>>
{
    public Guid UserGuid { get; set; }
    public long UserId { get; set; }
}
