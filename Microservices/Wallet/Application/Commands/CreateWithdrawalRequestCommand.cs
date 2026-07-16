using CryptoJackpot.Wallet.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Commands;

/// <summary>
/// Creates a withdrawal request, blocking the requested amount from the user's available balance.
/// Requires verification via 2FA code or email verification code.
/// </summary>
public class CreateWithdrawalRequestCommand : IRequest<Result<WithdrawalRequestDto>>
{
    public Guid UserGuid { get; set; }
    public decimal Amount { get; set; }
    public Guid WalletGuid { get; set; }

    /// <summary>TOTP code from authenticator app (if 2FA enabled).</summary>
    public string? TwoFactorCode { get; set; }

    /// <summary>Email verification code (if 2FA not enabled).</summary>
    public string? EmailVerificationCode { get; set; }
}
