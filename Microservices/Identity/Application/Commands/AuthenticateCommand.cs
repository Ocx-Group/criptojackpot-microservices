using CryptoJackpot.Identity.Application.DTOs;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class AuthenticateCommand : IRequest<ResultResponse<UserDto?>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
