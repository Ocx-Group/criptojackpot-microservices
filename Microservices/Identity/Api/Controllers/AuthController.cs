using CryptoJackpot.Identity.Application.DTOs;
using CryptoJackpot.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Identity.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request)
    {
        var result = await _authService.AuthenticateAsync(request);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ResultResponse<T> result)
    {
        if (!result.Success)
        {
            var statusCode = result.ErrorType switch
            {
                ErrorType.Unauthorized => 401,
                ErrorType.Forbidden => 403,
                ErrorType.NotFound => 404,
                ErrorType.BadRequest => 400,
                _ => 500
            };

            return StatusCode(statusCode, new { success = false, message = result.Message });
        }

        return Ok(new { success = true, data = result.Data });
    }
}
