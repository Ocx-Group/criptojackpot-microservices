using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Application.Requests;
using CryptoJackpot.Wallet.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Wallet.Api.Controllers;

[ApiController]
[Route("api/v1/withdrawals")]
[Authorize]
public class WithdrawalController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public WithdrawalController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Sends a 6-digit verification code to the user's email.
    /// Only for users without 2FA enabled.
    /// </summary>
    [HttpPost("request-code")]
    public async Task<IActionResult> RequestVerificationCode(CancellationToken cancellationToken)
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var command = new RequestWithdrawalVerificationCodeCommand
        {
            UserGuid = userGuid.Value,
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a withdrawal request. Blocks the requested amount from the user's available balance.
    /// Requires verification via 2FA code or email verification code.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWithdrawal(
        [FromBody] CreateWithdrawalRequestRequest request,
        CancellationToken cancellationToken)
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var command = new CreateWithdrawalRequestCommand
        {
            UserGuid = userGuid.Value,
            Amount = request.Amount,
            WalletGuid = request.WalletGuid,
            TwoFactorCode = request.TwoFactorCode,
            EmailVerificationCode = request.EmailVerificationCode,
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    // ── Admin endpoints ─────────────────────────────────────────────────

    /// <summary>
    /// Lists all withdrawal requests with optional status filter. Admin only.
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllWithdrawalRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] WithdrawalRequestStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllWithdrawalRequestsQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Approves a pending withdrawal request. Admin only.
    /// </summary>
    [HttpPost("admin/{requestGuid:guid}/approve")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ApproveWithdrawal(
        Guid requestGuid,
        [FromBody] ProcessWithdrawalRequestRequest? request,
        CancellationToken cancellationToken)
    {
        var command = new ProcessWithdrawalRequestCommand
        {
            RequestGuid = requestGuid,
            Approve = true,
            AdminNotes = request?.AdminNotes,
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Rejects a pending withdrawal request and refunds the blocked funds. Admin only.
    /// </summary>
    [HttpPost("admin/{requestGuid:guid}/reject")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RejectWithdrawal(
        Guid requestGuid,
        [FromBody] ProcessWithdrawalRequestRequest? request,
        CancellationToken cancellationToken)
    {
        var command = new ProcessWithdrawalRequestCommand
        {
            RequestGuid = requestGuid,
            Approve = false,
            AdminNotes = request?.AdminNotes,
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
