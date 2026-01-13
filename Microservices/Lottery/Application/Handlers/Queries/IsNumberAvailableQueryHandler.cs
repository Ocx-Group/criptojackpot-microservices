using CryptoJackpot.Lottery.Application.Queries;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Queries;

public class IsNumberAvailableQueryHandler : IRequestHandler<IsNumberAvailableQuery, Result<bool>>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILogger<IsNumberAvailableQueryHandler> _logger;

    public IsNumberAvailableQueryHandler(
        ILotteryNumberRepository lotteryNumberRepository,
        ILogger<IsNumberAvailableQueryHandler> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(IsNumberAvailableQuery request, CancellationToken cancellationToken)
    {
        var isAvailable = await _lotteryNumberRepository.IsNumberAvailableAsync(
            request.LotteryId, request.Number, request.Series);

        _logger.LogDebug("Number {Number} series {Series} for lottery {LotteryId} availability: {IsAvailable}",
            request.Number, request.Series, request.LotteryId, isAvailable);

        return Result.Ok(isAvailable);
    }
}

