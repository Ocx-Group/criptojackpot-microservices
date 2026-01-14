using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Domain.Exceptions;
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

            // OPTIMIZACIÓN: Verificar disponibilidad de TODOS los números en UNA SOLA consulta
            // Esto elimina el problema N+1 (antes: N consultas, ahora: 1 consulta)
            var alreadyReserved = await _lotteryNumberRepository.GetAlreadyReservedNumbersAsync(
                request.LotteryId, 
                request.Series, 
                request.Numbers);

            if (alreadyReserved.Any())
            {
                return Result.Fail<List<LotteryNumberDto>>(new ConflictError(
                    $"Numbers already sold: {string.Join(", ", alreadyReserved)} in series {request.Series}"));
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

            try
            {
                await _lotteryNumberRepository.AddRangeAsync(lotteryNumbers);
            }
            catch (DuplicateNumberReservationException ex)
            {
                // Manejo de concurrencia: Si dos usuarios intentan reservar el mismo número
                // simultáneamente, el repositorio lanza esta excepción de dominio.
                _logger.LogWarning(ex, 
                    "Concurrent reservation conflict for ticket {TicketId} in lottery {LotteryId}", 
                    request.TicketId, request.LotteryId);
                
                return Result.Fail<List<LotteryNumberDto>>(new ConflictError(
                    "One or more numbers were reserved by another user. Please try again with different numbers."));
            }

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
