using AutoMapper;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.IntegrationEvents.Winner;
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
    private readonly IEventBus _eventBus;
    private readonly IMapper _mapper;
    private readonly ILogger<DetermineWinnerCommandHandler> _logger;

    public DetermineWinnerCommandHandler(
        IWinnerRepository winnerRepository,
        ITicketSearchGrpcClient ticketSearchClient,
        IEventBus eventBus,
        IMapper mapper,
        ILogger<DetermineWinnerCommandHandler> logger)
    {
        _winnerRepository = winnerRepository;
        _ticketSearchClient = ticketSearchClient;
        _eventBus = eventBus;
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

        try
        {
            await _eventBus.Publish(new WinnerDeterminedEvent
            {
                WinnerGuid = created.WinnerGuid,
                LotteryId = created.LotteryId,
                LotteryTitle = created.LotteryTitle,
                Number = created.Number,
                Series = created.Series,
                UserId = created.UserId,
                UserName = created.UserName,
                UserEmail = created.UserEmail,
                PrizeName = created.PrizeName,
                PrizeEstimatedValue = created.PrizeEstimatedValue,
                PrizeImageUrl = created.PrizeImageUrl,
                PurchaseAmount = created.PurchaseAmount,
                WonAt = created.WonAt,
                LotteryType = request.LotteryType
            });
            _logger.LogInformation(
                "WinnerDeterminedEvent published for winner {WinnerGuid}, lottery {LotteryId}",
                created.WinnerGuid, created.LotteryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish WinnerDeterminedEvent for winner {WinnerGuid}",
                created.WinnerGuid);
        }

        return ResultExtensions.Created(_mapper.Map<WinnerDto>(created));
    }
}
