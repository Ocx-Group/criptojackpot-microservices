// This file is kept for backward compatibility.
// Use CryptoJackpot.Domain.Core.Extensions.ResultResponseExtensions instead.

using CryptoJackpot.Domain.Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Identity.Api.Extensions;

/// <summary>
/// Wrapper for shared ResultResponseExtensions.
/// Consider importing CryptoJackpot.Domain.Core.Extensions directly.
/// </summary>
public static class ResultResponseExtensions
{
    public static IActionResult ToActionResult<T>(this ResultResponse<T> result)
        => CryptoJackpot.Domain.Core.Extensions.ResultResponseExtensions.ToActionResult(result);
}
