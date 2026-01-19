using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Queries;

public class IsNumberAvailableQueryHandler : IRequestHandler<IsNumberAvailableQuery, Result<bool>>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly ILogger<IsNumberAvailableQueryHandler> _logger;

    public IsNumberAvailableQueryHandler(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryDrawRepository lotteryDrawRepository,
        ILogger<IsNumberAvailableQueryHandler> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _lotteryDrawRepository = lotteryDrawRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(IsNumberAvailableQuery request, CancellationToken cancellationToken)
    {
        var lottery = await _lotteryDrawRepository.GetLotteryByGuidAsync(request.LotteryId);
        if (lottery is null)
            return Result.Fail<bool>(new NotFoundError("Lottery not found"));

        var isAvailable = await _lotteryNumberRepository.IsNumberAvailableAsync(
            lottery.Id, request.Number, request.Series);

        _logger.LogDebug("Number {Number} series {Series} for lottery {LotteryId} availability: {IsAvailable}",
            request.Number, request.Series, request.LotteryId, isAvailable);

        return Result.Ok(isAvailable);
    }
}

