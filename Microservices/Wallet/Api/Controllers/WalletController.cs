using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Wallet.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Wallet.Api.Controllers;

[ApiController]
[Route("api/v1/wallets")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IMediator _mediator;

    public WalletController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        return Ok(new { message = "Wallet API is running" });
    }

    [HttpGet("referral-earnings")]
    public async Task<IActionResult> GetReferralEarnings(CancellationToken cancellationToken)
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var query = new GetReferralEarningsQuery { UserGuid = userGuid.Value };
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }
}

