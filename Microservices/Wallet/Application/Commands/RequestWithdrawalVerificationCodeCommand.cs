using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Commands;

/// <summary>
/// Sends a 6-digit verification code to the user's email for withdrawal authorization.
/// Only used when the user does NOT have 2FA enabled.
/// </summary>
public class RequestWithdrawalVerificationCodeCommand : IRequest<Result<bool>>
{
    public Guid UserGuid { get; set; }
}
