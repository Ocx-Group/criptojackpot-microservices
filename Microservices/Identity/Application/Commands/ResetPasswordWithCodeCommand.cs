using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.DTOs;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class ResetPasswordWithCodeCommand : IRequest<ResultResponse<UserDto?>>
{
    public string Email { get; set; } = null!;
    public string SecurityCode { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}
