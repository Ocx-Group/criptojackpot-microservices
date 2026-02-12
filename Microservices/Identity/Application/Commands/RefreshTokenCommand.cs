using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

/// <summary>
/// Command to refresh access token using a valid refresh token.
/// Implements rotation: old token is revoked, new one is issued with same FamilyId.
/// </summary>
public class RefreshTokenCommand : IRequest<Result<TokenRotationResultDto>>
{
    /// <summary>
    /// Raw refresh token from HttpOnly cookie.
    /// </summary>
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// Client IP address for audit trail.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Device info/User-Agent for session tracking.
    /// </summary>
    public string? DeviceInfo { get; set; }
}

