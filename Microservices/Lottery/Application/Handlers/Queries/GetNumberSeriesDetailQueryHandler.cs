using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Queries;

public class GetNumberSeriesDetailQueryHandler : IRequestHandler<GetNumberSeriesDetailQuery, Result<NumberSeriesDetailDto>>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly ILogger<GetNumberSeriesDetailQueryHandler> _logger;

    public GetNumberSeriesDetailQueryHandler(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryDrawRepository lotteryDrawRepository,
        ILogger<GetNumberSeriesDetailQueryHandler> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _lotteryDrawRepository = lotteryDrawRepository;
        _logger = logger;
    }

    public async Task<Result<NumberSeriesDetailDto>> Handle(GetNumberSeriesDetailQuery request, CancellationToken cancellationToken)
    {
        var lottery = await _lotteryDrawRepository.GetLotteryByGuidAsync(request.LotteryId);
        if (lottery is null)
            return Result.Fail<NumberSeriesDetailDto>(new NotFoundError("Lottery not found"));

        if (request.Number < lottery.MinNumber || request.Number > lottery.MaxNumber)
            return Result.Fail<NumberSeriesDetailDto>(new NotFoundError("Number out of range"));

        var records = await _lotteryNumberRepository.GetSeriesForNumberAsync(lottery.Id, request.Number);
        var recordMap = records.ToDictionary(r => r.Series, r => r.Status.ToString());

        var seriesList = new List<SeriesStatusItemDto>();
        var sold = 0;
        var reserved = 0;

        for (var s = 1; s <= lottery.TotalSeries; s++)
        {
            var status = recordMap.TryGetValue(s, out var st) ? st : "Available";
            seriesList.Add(new SeriesStatusItemDto { Series = s, Status = status });

            if (status == "Sold") sold++;
            else if (status == "Reserved") reserved++;
        }

        var result = new NumberSeriesDetailDto
        {
            LotteryId = request.LotteryId,
            Number = request.Number,
            TotalSeries = lottery.TotalSeries,
            SoldCount = sold,
            ReservedCount = reserved,
            AvailableCount = lottery.TotalSeries - sold - reserved,
            Series = seriesList
        };

        _logger.LogInformation("Retrieved series detail for number {Number} in lottery {LotteryId}",
            request.Number, request.LotteryId);

        return Result.Ok(result);
    }
}
