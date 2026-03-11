using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.Requests;
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
}
