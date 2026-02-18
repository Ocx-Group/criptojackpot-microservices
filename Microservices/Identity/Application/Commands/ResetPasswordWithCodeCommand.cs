using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class ResetPasswordWithCodeCommand : IRequest<Result<bool>>
{
    public string Email { get; set; } = null!;
    public string SecurityCode { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}
