using CryptoJackpot.Domain.Core.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Domain.Core.Extensions;

/// <summary>
/// Shared extension methods for converting ResultResponse to IActionResult.
/// Used across all microservices for consistent API responses.
/// </summary>
public static class ResultResponseExtensions
{
    public static IActionResult ToActionResult<T>(this ResultResponse<T> result)
    {
        if (!result.Success)
        {
            var statusCode = result.ErrorType switch
            {
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.BadRequest => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            return new ObjectResult(new { success = false, message = result.Message })
            {
                StatusCode = statusCode
            };
        }

        var successStatusCode = result.SuccessType switch
        {
            SuccessType.Created => StatusCodes.Status201Created,
            SuccessType.NoContent => StatusCodes.Status204NoContent,
            _ => StatusCodes.Status200OK
        };

        if (result.SuccessType == SuccessType.NoContent)
        {
            return new NoContentResult();
        }

        return new ObjectResult(new { success = true, data = result.Data })
        {
            StatusCode = successStatusCode
        };
    }
}

