using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Queries;

public class GetAvailableNumbersQueryHandler : IRequestHandler<GetAvailableNumbersQuery, Result<List<LotteryNumberDto>>>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly ILogger<GetAvailableNumbersQueryHandler> _logger;

    public GetAvailableNumbersQueryHandler(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryDrawRepository lotteryDrawRepository,
        ILogger<GetAvailableNumbersQueryHandler> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _lotteryDrawRepository = lotteryDrawRepository;
        _logger = logger;
    }

    public async Task<Result<List<LotteryNumberDto>>> Handle(GetAvailableNumbersQuery request, CancellationToken cancellationToken)
    {
        var lottery = await _lotteryDrawRepository.GetLotteryByGuidAsync(request.LotteryId);
        if (lottery is null)
            return Result.Fail<List<LotteryNumberDto>>(new NotFoundError("Lottery not found"));

        var maxNumber = lottery.MaxNumber - lottery.MinNumber + 1;
        var availableNumbers = await _lotteryNumberRepository.GetRandomAvailableNumbersAsync(
            lottery.Id, request.Count, maxNumber, lottery.MinNumber);

        var result = availableNumbers.Select(n => new LotteryNumberDto
        {
            Number = n,
            LotteryId = request.LotteryId,
            IsAvailable = true
        }).ToList();

        _logger.LogInformation("Retrieved {Count} available numbers for lottery {LotteryId}", result.Count, request.LotteryId);

        return Result.Ok(result);
    }
}
