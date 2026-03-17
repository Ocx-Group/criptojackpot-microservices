using AutoMapper;
using CryptoJackpot.Winner.Application.DTOs;
using CryptoJackpot.Winner.Application.Queries;
using CryptoJackpot.Winner.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Winner.Application.Handlers.Queries;

public class GetAllWinnersQueryHandler : IRequestHandler<GetAllWinnersQuery, Result<IEnumerable<WinnerDto>>>
{
    private readonly IWinnerRepository _winnerRepository;
    private readonly IMapper _mapper;

    public GetAllWinnersQueryHandler(IWinnerRepository winnerRepository, IMapper mapper)
    {
        _winnerRepository = winnerRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<WinnerDto>>> Handle(
        GetAllWinnersQuery request,
        CancellationToken cancellationToken)
    {
        var winners = await _winnerRepository.GetAllAsync();
        return Result.Ok(_mapper.Map<IEnumerable<WinnerDto>>(winners));
    }
}
