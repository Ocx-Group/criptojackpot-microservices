using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Queries;

public class GetNumberStatsQueryHandler : IRequestHandler<GetNumberStatsQuery, Result<LotteryNumberStatsDto>>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly ILogger<GetNumberStatsQueryHandler> _logger;

    public GetNumberStatsQueryHandler(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryDrawRepository lotteryDrawRepository,
        ILogger<GetNumberStatsQueryHandler> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _lotteryDrawRepository = lotteryDrawRepository;
        _logger = logger;
    }

    public async Task<Result<LotteryNumberStatsDto>> Handle(GetNumberStatsQuery request, CancellationToken cancellationToken)
    {
        var lottery = await _lotteryDrawRepository.GetLotteryByIdAsync(request.LotteryId);
        if (lottery is null)
            return Result.Fail<LotteryNumberStatsDto>(new NotFoundError("Lottery not found"));

        var soldNumbers = await _lotteryNumberRepository.GetSoldNumbersAsync(request.LotteryId);
        var totalPossible = (lottery.MaxNumber - lottery.MinNumber + 1) * lottery.TotalSeries;

        var stats = new LotteryNumberStatsDto
        {
            LotteryId = request.LotteryId,
            TotalNumbers = totalPossible,
            SoldNumbers = soldNumbers.Count,
            AvailableNumbers = totalPossible - soldNumbers.Count,
            PercentageSold = totalPossible > 0
                ? Math.Round((decimal)soldNumbers.Count / totalPossible * 100, 2)
                : 0
        };

        _logger.LogInformation("Retrieved stats for lottery {LotteryId}: {SoldNumbers}/{TotalNumbers} sold",
            request.LotteryId, stats.SoldNumbers, stats.TotalNumbers);

        return Result.Ok(stats);
    }
}

