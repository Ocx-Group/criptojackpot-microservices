using Asp.Versioning;
using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.Queries;
using CryptoJackpot.Order.Application.Requests;
using CryptoJackpot.Order.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Order.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public OrderController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Completes an order after successful payment.
    /// Creates a ticket (confirmed purchase) from the order.
    /// Must be called within 5 minutes of order creation.
    /// </summary>
    [HttpPost("{orderId:guid}/complete")]
    public async Task<IActionResult> CompleteOrder([FromRoute] Guid orderId, [FromBody] CompleteOrderRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        var command = _mapper.Map<CompleteOrderCommand>(request);
        command.OrderId = orderId;
        command.UserId = userId.Value;

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a CoinPayments invoice for a pending order.
    /// Returns the checkout URL to redirect the user for payment.
    /// </summary>
    [HttpPost("{orderId:guid}/pay")]
    public async Task<IActionResult> PayOrder([FromRoute] Guid orderId)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        var command = new PayOrderCommand
        {
            OrderId = orderId,
            UserId = userId.Value
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Returns the list of supported cryptocurrencies for payment.
    /// Cached for 6 hours since the list rarely changes.
    /// </summary>
    [HttpGet("currencies")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrencies()
    {
        var result = await _mediator.Send(new GetCurrenciesQuery());
        return result.ToActionResult();
    }

    /// <summary>
    /// Returns all orders with details (admin only). Supports pagination and status filter.
    /// </summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllOrdersQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Cancels a pending order and releases reserved lottery numbers.
    /// </summary>
    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid orderId)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        var command = new CancelOrderCommand
        {
            OrderId = orderId,
            UserId = userId.Value,
            Reason = "User cancelled"
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets order/ticket statistics for admin dashboard.
    /// </summary>
    [HttpGet("admin/stats")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetOrderStats()
    {
        var result = await _mediator.Send(new GetOrderStatsQuery());
        return result.ToActionResult();
    }

    /// <summary>
    /// Pays for a pending order using the user's internal wallet balance.
    /// Debits the wallet and creates tickets instantly (no external payment gateway).
    /// </summary>
    [HttpPost("{orderId:guid}/pay-with-balance")]
    public async Task<IActionResult> PayOrderWithBalance([FromRoute] Guid orderId)
    {
        var userId = User.GetUserId();
        var userGuid = User.GetUserGuid();
        if (userId is null || userGuid is null)
            return Unauthorized();

        var command = new PayOrderWithBalanceCommand
        {
            OrderId = orderId,
            UserId = userId.Value,
            UserGuid = userGuid.Value
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
