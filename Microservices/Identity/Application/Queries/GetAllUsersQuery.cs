using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Identity.Application.DTOs;
using MediatR;

namespace CryptoJackpot.Identity.Application.Queries;

public class GetAllUsersQuery : IRequest<ResultResponse<IEnumerable<UserDto>>>
{
    public long? ExcludeUserId { get; set; }
}
