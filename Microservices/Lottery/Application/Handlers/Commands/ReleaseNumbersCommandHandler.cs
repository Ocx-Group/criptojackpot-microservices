using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Commands;

public class ReleaseNumbersCommandHandler : IRequestHandler<ReleaseNumbersCommand, Result<bool>>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILogger<ReleaseNumbersCommandHandler> _logger;

    public ReleaseNumbersCommandHandler(
        ILotteryNumberRepository lotteryNumberRepository,
        ILogger<ReleaseNumbersCommandHandler> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ReleaseNumbersCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var released = await _lotteryNumberRepository.ReleaseNumbersByTicketAsync(request.TicketId);

            if (!released)
            {
                _logger.LogWarning("No numbers found to release for ticket {TicketId}", request.TicketId);
                return Result.Fail<bool>(new NotFoundError("No numbers found to release"));
            }

            _logger.LogInformation("Released numbers for ticket {TicketId}", request.TicketId);
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release numbers for ticket {TicketId}", request.TicketId);
            return Result.Fail<bool>(new InternalServerError("Failed to release numbers"));
        }
    }
}

