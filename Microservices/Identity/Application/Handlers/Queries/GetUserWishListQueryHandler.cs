using AutoMapper;
using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Handlers.Queries;

public class GetUserWishListQueryHandler : IRequestHandler<GetUserWishListQuery, Result<List<WishListItemDto>>>
{
    private readonly IWishListRepository _wishListRepository;
    private readonly IMapper _mapper;

    public GetUserWishListQueryHandler(
        IWishListRepository wishListRepository,
        IMapper mapper)
    {
        _wishListRepository = wishListRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<WishListItemDto>>> Handle(GetUserWishListQuery request, CancellationToken cancellationToken)
    {
        var items = await _wishListRepository.GetByUserIdAsync(request.UserId);
        var dtos = _mapper.Map<List<WishListItemDto>>(items);
        return Result.Ok(dtos);
    }
}
