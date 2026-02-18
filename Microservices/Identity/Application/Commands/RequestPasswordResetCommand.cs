using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class RequestPasswordResetCommand : IRequest<Result<bool>>
{
    public string Email { get; set; } = null!;
}
