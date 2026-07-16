using Asp.Versioning;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/wishlists")]
[Authorize]
public class WishListController : ControllerBase
{
    private readonly IMediator _mediator;

    public WishListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the current user's wishlist.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWishList()
    {
        var userGuid = User.GetUserGuid();
        var userId = User.GetUserId();
        if (userGuid is null || userId is null)
            return Unauthorized();

        var query = new GetUserWishListQuery
        {
            UserGuid = userGuid.Value,
            UserId = userId.Value
        };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Add a lottery to the current user's wishlist.
    /// </summary>
    [HttpPost("{lotteryGuid:guid}")]
    public async Task<IActionResult> AddToWishList([FromRoute] Guid lotteryGuid)
    {
        var userGuid = User.GetUserGuid();
        var userId = User.GetUserId();
        if (userGuid is null || userId is null)
            return Unauthorized();

        var command = new AddToWishListCommand
        {
            UserGuid = userGuid.Value,
            UserId = userId.Value,
            LotteryGuid = lotteryGuid
        };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Remove a lottery from the current user's wishlist.
    /// </summary>
    [HttpDelete("{lotteryGuid:guid}")]
    public async Task<IActionResult> RemoveFromWishList([FromRoute] Guid lotteryGuid)
    {
        var userGuid = User.GetUserGuid();
        var userId = User.GetUserId();
        if (userGuid is null || userId is null)
            return Unauthorized();

        var command = new RemoveFromWishListCommand
        {
            UserGuid = userGuid.Value,
            UserId = userId.Value,
            LotteryGuid = lotteryGuid
        };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
