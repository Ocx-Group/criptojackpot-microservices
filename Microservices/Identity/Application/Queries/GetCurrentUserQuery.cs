using CryptoJackpot.Identity.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Identity.Application.Queries;

public class GetCurrentUserQuery : IRequest<Result<UserDto>>
{
    public Guid UserGuid { get; set; }
}
