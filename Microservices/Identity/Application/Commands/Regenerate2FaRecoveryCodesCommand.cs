using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

/// <summary>
/// Command to regenerate recovery codes for a user with 2FA enabled.
/// Invalidates all existing recovery codes and generates new ones.
/// Requires verification with current TOTP code.
/// </summary>
public class Regenerate2FaRecoveryCodesCommand : IRequest<Result<Confirm2FaResultDto>>
{
    /// <summary>
    /// User GUID from JWT claims.
    /// </summary>
    public Guid UserGuid { get; set; }

    /// <summary>
    /// Current 6-digit TOTP code from authenticator app.
    /// Required to verify user has access to authenticator.
    /// </summary>
    public string Code { get; set; } = null!;
}

