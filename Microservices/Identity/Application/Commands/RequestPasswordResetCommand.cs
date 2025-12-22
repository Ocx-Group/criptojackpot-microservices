using CryptoJackpot.Domain.Core.Responses;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class RequestPasswordResetCommand : IRequest<ResultResponse<string>>
{
    public string Email { get; set; } = null!;
}
