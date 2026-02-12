using Asp.Versioning;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Queries;
using CryptoJackpot.Identity.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetById([FromRoute] long userId)
    {
        var query = new GetUserByIdQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpGet("get-all-users")]
    public async Task<IActionResult> GetAll([FromQuery] long excludeUserId)
    {
        var query = new GetAllUsersQuery { ExcludeUserId = excludeUserId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    [HttpPut("{userId:long}")]
    public async Task<IActionResult> Update(long userId, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand
        {
            UserId = userId,
            Name = request.Name,
            LastName = request.LastName,
            Phone = request.Phone
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("{userId:long}/image/upload-url")]
    public async Task<IActionResult> GenerateUploadUrl(long userId, [FromBody] GenerateUploadUrlRequest request)
    {
        var command = new GenerateUploadUrlCommand
        {
            UserId = userId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            ExpirationMinutes = request.ExpirationMinutes
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPatch("update-image-profile")]
    public async Task<IActionResult> UpdateImage([FromBody] UpdateUserImageRequest request)
    {
        var command = new UpdateUserImageCommand
        {
            UserId = request.UserId,
            StorageKey = request.StorageKey
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
