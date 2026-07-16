using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Identity.Application.Handlers.Commands;

public class AddToWishListCommandHandler : IRequestHandler<AddToWishListCommand, Result<WishListItemDto>>
{
    private readonly IWishListRepository _wishListRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AddToWishListCommandHandler> _logger;

    public AddToWishListCommandHandler(
        IWishListRepository wishListRepository,
        IMapper mapper,
        ILogger<AddToWishListCommandHandler> logger)
    {
        _wishListRepository = wishListRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<WishListItemDto>> Handle(AddToWishListCommand request, CancellationToken cancellationToken)
    {
        var exists = await _wishListRepository.ExistsAsync(request.UserId, request.LotteryGuid);
        if (exists)
        {
            _logger.LogWarning("Lottery {LotteryGuid} already in wishlist for user {UserGuid}", 
                request.LotteryGuid, request.UserGuid);
            return Result.Fail<WishListItemDto>("Lottery is already in your wishlist");
        }

        var item = new WishListItem
        {
            UserGuid = request.UserGuid,
            UserId = request.UserId,
            LotteryGuid = request.LotteryGuid
        };

        var created = await _wishListRepository.AddAsync(item);

        _logger.LogInformation("Lottery {LotteryGuid} added to wishlist for user {UserGuid}", 
            request.LotteryGuid, request.UserGuid);

        return ResultExtensions.Created(_mapper.Map<WishListItemDto>(created));
    }
}
