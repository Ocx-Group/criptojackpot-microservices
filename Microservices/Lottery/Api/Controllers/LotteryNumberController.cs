using Asp.Versioning;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Lottery.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/lottery-number")]
public class LotteryNumberController : ControllerBase
{
    private readonly IMediator _mediator;

    public LotteryNumberController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("{lotteryId:guid}/available")]
    public async Task<IActionResult> GetAvailableNumbers([FromRoute] Guid lotteryId, [FromQuery] int count = 10)
    {
        var query = new GetAvailableNumbersQuery
        {
            LotteryId = lotteryId,
            Count = count
        };

        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("{lotteryId:guid}/check")]
    public async Task<IActionResult> IsNumberAvailable(
        [FromRoute] Guid lotteryId,
        [FromQuery] int number,
        [FromQuery] int series)
    {
        var query = new IsNumberAvailableQuery
        {
            LotteryId = lotteryId,
            Number = number,
            Series = series
        };

        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpPost("{lotteryId:guid}/reserve")]
    public async Task<IActionResult> ReserveNumbers(
        [FromRoute] Guid lotteryId,
        [FromBody] ReserveNumbersRequest request)
    {
        var command = new ReserveNumbersCommand
        {
            LotteryId = lotteryId,
            TicketId = request.TicketId,
            Numbers = request.Numbers,
            Series = request.Series
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpDelete("release/{ticketId:guid}")]
    public async Task<IActionResult> ReleaseNumbers([FromRoute] Guid ticketId)
    {
        var command = new ReleaseNumbersCommand
        {
            TicketId = ticketId
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("{lotteryId:guid}/stats")]
    public async Task<IActionResult> GetNumberStats([FromRoute] Guid lotteryId)
    {
        var query = new GetNumberStatsQuery
        {
            LotteryId = lotteryId
        };

        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }
}

