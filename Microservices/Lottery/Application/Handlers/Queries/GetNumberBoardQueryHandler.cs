using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Queries;

public class GetNumberBoardQueryHandler : IRequestHandler<GetNumberBoardQuery, Result<NumberBoardSummaryDto>>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly ILogger<GetNumberBoardQueryHandler> _logger;

    public GetNumberBoardQueryHandler(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryDrawRepository lotteryDrawRepository,
        ILogger<GetNumberBoardQueryHandler> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _lotteryDrawRepository = lotteryDrawRepository;
        _logger = logger;
    }

    public async Task<Result<NumberBoardSummaryDto>> Handle(GetNumberBoardQuery request, CancellationToken cancellationToken)
    {
        var lottery = await _lotteryDrawRepository.GetLotteryByGuidAsync(request.LotteryId);
        if (lottery is null)
            return Result.Fail<NumberBoardSummaryDto>(new NotFoundError("Lottery not found"));

        var statusCounts = await _lotteryNumberRepository.GetStatusCountsPerNumberAsync(lottery.Id);

        var minNumber = lottery.MinNumber;
        var maxNumber = lottery.MaxNumber;
        var totalSeries = lottery.TotalSeries;
        var numbers = new List<NumberSummaryItemDto>();

        var totalSold = 0;
        var totalReserved = 0;

        for (var n = minNumber; n <= maxNumber; n++)
        {
            var sold = 0;
            var reserved = 0;

            if (statusCounts.TryGetValue(n, out var counts))
            {
                sold = counts.Sold;
                reserved = counts.Reserved;
            }

            var available = totalSeries - sold - reserved;

            numbers.Add(new NumberSummaryItemDto
            {
                Number = n,
                SoldCount = sold,
                ReservedCount = reserved,
                AvailableCount = available
            });

            totalSold += sold;
            totalReserved += reserved;
        }

        var totalSlots = (maxNumber - minNumber + 1) * totalSeries;

        var result = new NumberBoardSummaryDto
        {
            LotteryId = request.LotteryId,
            MinNumber = minNumber,
            MaxNumber = maxNumber,
            TotalSeries = totalSeries,
            TotalSlots = totalSlots,
            SoldCount = totalSold,
            ReservedCount = totalReserved,
            AvailableCount = totalSlots - totalSold - totalReserved,
            Numbers = numbers
        };

        _logger.LogInformation("Retrieved number board for lottery {LotteryId}: {Sold}/{Total} sold",
            request.LotteryId, totalSold, totalSlots);

        return Result.Ok(result);
    }
}
