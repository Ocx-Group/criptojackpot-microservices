using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Winner.Application.Commands;
using CryptoJackpot.Winner.Application.DTOs;
using CryptoJackpot.Winner.Domain.Interfaces;
using CryptoJackpot.Winner.Domain.Models;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Winner.Application.Handlers.Commands;

public class DetermineWinnerCommandHandler : IRequestHandler<DetermineWinnerCommand, Result<WinnerDto>>
{
    private readonly IWinnerRepository _winnerRepository;
    private readonly ITicketSearchGrpcClient _ticketSearchClient;

    public DetermineWinnerCommandHandler(
        IWinnerRepository winnerRepository,
        ITicketSearchGrpcClient ticketSearchClient)
    {
        _winnerRepository = winnerRepository;
        _ticketSearchClient = ticketSearchClient;
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
        var ticket = await _ticketSearchClient.SearchTicketAsync(
            request.LotteryId, request.Number, request.Series, cancellationToken);

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

        var dto = MapToDto(created);
        return ResultExtensions.Created(dto);
    }

    private static WinnerDto MapToDto(LotteryWinner winner) => new()
    {
        WinnerGuid = winner.WinnerGuid,
        LotteryId = winner.LotteryId,
        LotteryTitle = winner.LotteryTitle,
        Number = winner.Number,
        Series = winner.Series,
        TicketGuid = winner.TicketGuid,
        PurchaseAmount = winner.PurchaseAmount,
        UserId = winner.UserId,
        UserName = winner.UserName,
        UserEmail = winner.UserEmail,
        PrizeName = winner.PrizeName,
        PrizeEstimatedValue = winner.PrizeEstimatedValue,
        PrizeImageUrl = winner.PrizeImageUrl,
        Status = winner.Status,
        WonAt = winner.WonAt,
        CreatedAt = winner.CreatedAt
    };
}
