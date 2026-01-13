using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Domain.Interfaces;
using CryptoJackpot.Lottery.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Lottery.Application.Handlers.Commands;

public class ReserveNumbersCommandHandler : IRequestHandler<ReserveNumbersCommand, Result<List<LotteryNumberDto>>>
{
    private readonly ILotteryNumberRepository _lotteryNumberRepository;
    private readonly ILotteryDrawRepository _lotteryDrawRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReserveNumbersCommandHandler> _logger;

    public ReserveNumbersCommandHandler(
        ILotteryNumberRepository lotteryNumberRepository,
        ILotteryDrawRepository lotteryDrawRepository,
        IMapper mapper,
        ILogger<ReserveNumbersCommandHandler> logger)
    {
        _lotteryNumberRepository = lotteryNumberRepository;
        _lotteryDrawRepository = lotteryDrawRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<LotteryNumberDto>>> Handle(ReserveNumbersCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var lottery = await _lotteryDrawRepository.GetLotteryByIdAsync(request.LotteryId);
            if (lottery is null)
                return Result.Fail<List<LotteryNumberDto>>(new NotFoundError("Lottery not found"));

            // Validar que los números estén en el rango permitido
            var invalidNumbers = request.Numbers.Where(n => n < lottery.MinNumber || n > lottery.MaxNumber).ToList();
            if (invalidNumbers.Any())
                return Result.Fail<List<LotteryNumberDto>>(new BadRequestError(
                    $"Numbers out of range: {string.Join(", ", invalidNumbers)}"));

            // Validar que la serie sea válida
            if (request.Series < 1 || request.Series > lottery.TotalSeries)
                return Result.Fail<List<LotteryNumberDto>>(new BadRequestError(
                    $"Invalid series. Must be between 1 and {lottery.TotalSeries}"));

            // Verificar disponibilidad de todos los números
            foreach (var number in request.Numbers)
            {
                var isAvailable = await _lotteryNumberRepository.IsNumberAvailableAsync(request.LotteryId, number, request.Series);
                if (!isAvailable)
                    return Result.Fail<List<LotteryNumberDto>>(new ConflictError(
                        $"Number {number} series {request.Series} is no longer available"));
            }

            // Crear los registros de números reservados
            var lotteryNumbers = request.Numbers.Select(n => new LotteryNumber
            {
                Id = Guid.NewGuid(),
                LotteryId = request.LotteryId,
                Number = n,
                Series = request.Series,
                IsAvailable = false,
                TicketId = request.TicketId
            }).ToList();

            await _lotteryNumberRepository.AddRangeAsync(lotteryNumbers);

            _logger.LogInformation("Reserved {Count} numbers for ticket {TicketId} in lottery {LotteryId}",
                request.Numbers.Count, request.TicketId, request.LotteryId);

            var result = _mapper.Map<List<LotteryNumberDto>>(lotteryNumbers);
            return ResultExtensions.Created(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reserve numbers for ticket {TicketId}", request.TicketId);
            return Result.Fail<List<LotteryNumberDto>>(new InternalServerError("Failed to reserve numbers"));
        }
    }
}
