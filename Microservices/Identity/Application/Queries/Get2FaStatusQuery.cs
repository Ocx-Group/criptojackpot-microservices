using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Queries;

/// <summary>
/// Query to get 2FA status for a user.
/// Returns whether 2FA is enabled and remaining recovery codes.
/// </summary>
public class Get2FaStatusQuery : IRequest<Result<TwoFactorStatusDto>>
{
    /// <summary>
    /// User GUID from JWT claims.
    /// </summary>
    public Guid UserGuid { get; set; }
}

