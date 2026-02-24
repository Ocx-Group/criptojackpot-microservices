using Asp.Versioning;
using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Wallet.Api.Extensions;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Wallet.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/user-wallets")]
[Authorize]
public class UserCryptoWalletController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public UserCryptoWalletController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWallets(CancellationToken cancellationToken)
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var query = new GetUserCryptoWalletsQuery { UserGuid = userGuid.Value };
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateWallet(
        [FromBody] CreateUserCryptoWalletRequest request,
        CancellationToken cancellationToken)
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var command = _mapper.Map<CreateUserCryptoWalletCommand>(request);
        command.UserGuid = userGuid.Value;

        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{walletId:guid}")]
    public async Task<IActionResult> DeleteWallet(Guid walletId, CancellationToken cancellationToken)
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var command = new DeleteUserCryptoWalletCommand
        {
            UserGuid = userGuid.Value,
            WalletGuid = walletId,
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPatch("{walletId:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid walletId, CancellationToken cancellationToken)
    {
        var userGuid = User.GetUserGuid();
        if (userGuid is null)
            return Unauthorized();

        var command = new SetDefaultUserCryptoWalletCommand
        {
            UserGuid = userGuid.Value,
            WalletGuid = walletId,
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
