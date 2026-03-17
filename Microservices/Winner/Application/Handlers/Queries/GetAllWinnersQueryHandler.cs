using CryptoJackpot.Winner.Application.DTOs;
using CryptoJackpot.Winner.Application.Queries;
using CryptoJackpot.Winner.Domain.Interfaces;
using CryptoJackpot.Winner.Domain.Models;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Winner.Application.Handlers.Queries;

public class GetAllWinnersQueryHandler : IRequestHandler<GetAllWinnersQuery, Result<IEnumerable<WinnerDto>>>
{
    private readonly IWinnerRepository _winnerRepository;

    public GetAllWinnersQueryHandler(IWinnerRepository winnerRepository)
    {
        _winnerRepository = winnerRepository;
    }

    public async Task<Result<IEnumerable<WinnerDto>>> Handle(
        GetAllWinnersQuery request,
        CancellationToken cancellationToken)
    {
        var winners = await _winnerRepository.GetAllAsync();
        var dtos = winners.Select(MapToDto);
        return Result.Ok(dtos);
    }

    private static WinnerDto MapToDto(LotteryWinner winner) => new()
    {
        WinnerGuid = winner.WinnerGuid,
        LotteryId = winner.LotteryId,
        LotteryTitle = winner.LotteryTitle,
        Number = winner.Number,
        Series = winner.Series,
        TicketGuid = winner.TicketGuid,
        PurchaseAmount = winner.PurchaseAmount,
        UserId = winner.UserId,
        UserName = winner.UserName,
        UserEmail = winner.UserEmail,
        PrizeName = winner.PrizeName,
        PrizeEstimatedValue = winner.PrizeEstimatedValue,
        PrizeImageUrl = winner.PrizeImageUrl,
        Status = winner.Status,
        WonAt = winner.WonAt,
        CreatedAt = winner.CreatedAt
    };
}
