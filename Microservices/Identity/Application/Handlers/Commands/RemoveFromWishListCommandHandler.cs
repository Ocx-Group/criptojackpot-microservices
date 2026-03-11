using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class RemoveFromWishListCommandHandler : IRequestHandler<RemoveFromWishListCommand, Result>
{
    private readonly IWishListRepository _wishListRepository;
    private readonly ILogger<RemoveFromWishListCommandHandler> _logger;

    public RemoveFromWishListCommandHandler(
        IWishListRepository wishListRepository,
        ILogger<RemoveFromWishListCommandHandler> logger)
    {
        _wishListRepository = wishListRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveFromWishListCommand request, CancellationToken cancellationToken)
    {
        var item = await _wishListRepository.GetByUserAndLotteryAsync(request.UserId, request.LotteryGuid);

        if (item is null)
            return Result.Fail(new NotFoundError("Item not found in wishlist"));

        await _wishListRepository.RemoveAsync(item);

        _logger.LogInformation("Lottery {LotteryGuid} removed from wishlist for user {UserGuid}", 
            request.LotteryGuid, request.UserGuid);

        return Result.Ok();
    }
}
