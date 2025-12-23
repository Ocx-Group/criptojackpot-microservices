using CryptoJackpot.Identity.Api.Extensions;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Identity.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        return result.ToActionResult();
    }

    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetById(long userId)
    {
        var result = await _userService.GetUserByIdAsync(userId);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] long? excludeUserId = null)
    {
        var result = await _userService.GetAllUsersAsync(excludeUserId);
        return result.ToActionResult();
    }

    [HttpPut("{userId:long}")]
    public async Task<IActionResult> Update(long userId, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(userId, request);
        return result.ToActionResult();
    }

    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var result = await _userService.UpdatePasswordAsync(request);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("password/reset-request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
    {
        var result = await _userService.RequestPasswordResetAsync(request.Email);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithCodeRequest request)
    {
        var result = await _userService.ResetPasswordWithCodeAsync(request);
        return result.ToActionResult();
    }

    [HttpPost("{userId:long}/security-code")]
    public async Task<IActionResult> GenerateSecurityCode(long userId)
    {
        var result = await _userService.GenerateNewSecurityCodeAsync(userId);
        return result.ToActionResult();
    }
}

