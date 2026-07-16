using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Winner.Application.Commands;
using CryptoJackpot.Winner.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Winner.Api.Controllers;

[ApiController]
[Route("api/v1/winners")]
[Authorize]
public class WinnerController : ControllerBase
{
    private readonly IMediator _mediator;

    public WinnerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registers a lottery winner. Admin enters number + series verified against sold tickets.
    /// </summary>
    [HttpPost("determine")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DetermineWinner([FromBody] DetermineWinnerCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Lists all lottery winners. Public endpoint for the winners page.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllWinners()
    {
        var result = await _mediator.Send(new GetAllWinnersQuery());
        return result.ToActionResult();
    }
}
