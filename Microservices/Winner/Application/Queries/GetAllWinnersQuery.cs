using CryptoJackpot.Winner.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Winner.Application.Queries;

public class GetAllWinnersQuery : IRequest<Result<IEnumerable<WinnerDto>>>
{
}
