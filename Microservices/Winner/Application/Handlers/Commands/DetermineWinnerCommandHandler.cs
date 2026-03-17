using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Winner.Application.Commands;
using CryptoJackpot.Winner.Application.DTOs;
using CryptoJackpot.Winner.Domain.Interfaces;
using CryptoJackpot.Winner.Domain.Models;
using FluentResults;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Winner.Application.Handlers.Commands;

public class DetermineWinnerCommandHandler : IRequestHandler<DetermineWinnerCommand, Result<WinnerDto>>
{
    private readonly IWinnerRepository _winnerRepository;
    private readonly ITicketSearchGrpcClient _ticketSearchClient;
    private readonly IMapper _mapper;
    private readonly ILogger<DetermineWinnerCommandHandler> _logger;

    public DetermineWinnerCommandHandler(
        IWinnerRepository winnerRepository,
        ITicketSearchGrpcClient ticketSearchClient,
        IMapper mapper,
        ILogger<DetermineWinnerCommandHandler> logger)
    {
        _winnerRepository = winnerRepository;
        _ticketSearchClient = ticketSearchClient;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<WinnerDto>> Handle(DetermineWinnerCommand request, CancellationToken cancellationToken)
    {
        // Check if a winner already exists for this lottery + number + series
        var existing = await _winnerRepository.GetByLotteryNumberSeriesAsync(
            request.LotteryId, request.Number, request.Series);

        if (existing is not null)
            return Result.Fail<WinnerDto>(
                new ConflictError("Ya existe un ganador registrado con ese número y serie para esta lotería"));

        // Verify the ticket was sold via gRPC call to Order service
        TicketSearchResult? ticket;
        try
        {
            ticket = await _ticketSearchClient.SearchTicketAsync(
                request.LotteryId, request.Number, request.Series, cancellationToken);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC call to Order service failed: {Status}", ex.StatusCode);
            return Result.Fail<WinnerDto>(
                new BadRequestError("No se pudo verificar el boleto. El servicio de órdenes no está disponible."));
        }

        if (ticket is null)
            return Result.Fail<WinnerDto>(
                new NotFoundError("No se encontró un boleto vendido con ese número y serie para esta lotería"));

        if (ticket.Status != "Active")
            return Result.Fail<WinnerDto>(
                new BadRequestError($"El boleto no está activo. Estado actual: {ticket.Status}"));

        var winner = new LotteryWinner
        {
            LotteryId = request.LotteryId,
            LotteryTitle = request.LotteryTitle,
            Number = request.Number,
            Series = request.Series,
            TicketGuid = ticket.TicketGuid,
            UserId = ticket.UserId,
            PurchaseAmount = ticket.PurchaseAmount,
            UserName = ticket.UserName,
            UserEmail = ticket.UserEmail,
            PrizeName = request.PrizeName,
            PrizeEstimatedValue = request.PrizeEstimatedValue,
            PrizeImageUrl = request.PrizeImageUrl,
            WonAt = DateTime.UtcNow
        };

        var created = await _winnerRepository.CreateAsync(winner);

        return ResultExtensions.Created(_mapper.Map<WinnerDto>(created));
    }
}
