using System.Security.Claims;

namespace CryptoJackpot.Wallet.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserGuid(this ClaimsPrincipal principal)
    {
        var userGuidClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? principal.FindFirst("sub")?.Value;

        return Guid.TryParse(userGuidClaim, out var guid) ? guid : null;
    }

    public static Guid GetRequiredUserGuid(this ClaimsPrincipal principal)
    {
        return principal.GetUserGuid()
               ?? throw new UnauthorizedAccessException("User identifier not found in token.");
    }
}
