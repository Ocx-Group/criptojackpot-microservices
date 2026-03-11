using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class RemoveFromWishListCommand : IRequest<Result>
{
    public Guid UserGuid { get; set; }
    public long UserId { get; set; }
    public Guid LotteryGuid { get; set; }
}
