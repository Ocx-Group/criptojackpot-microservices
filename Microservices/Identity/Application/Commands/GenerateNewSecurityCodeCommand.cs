using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.DTOs;
using MediatR;

namespace CryptoJackpot.Identity.Application.Commands;

public class GenerateNewSecurityCodeCommand : IRequest<ResultResponse<UserDto?>>
{
    public long UserId { get; set; }
}
