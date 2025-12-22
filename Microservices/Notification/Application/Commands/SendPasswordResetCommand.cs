using CryptoJackpot.Domain.Core.Responses;
using MediatR;

namespace CryptoJackpot.Notification.Application.Commands;

public class SendPasswordResetCommand : IRequest<ResultResponse<bool>>
{
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string SecurityCode { get; set; } = null!;
}
