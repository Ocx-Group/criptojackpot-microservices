using CryptoJackpot.Domain.Core.Protos;
using CryptoJackpot.Order.Domain.Interfaces;
using Grpc.Core;

namespace CryptoJackpot.Order.Api.Services;

/// <summary>
/// gRPC server implementation for ticket search queries.
/// Allows the Winner microservice to verify sold tickets before determining winners.
/// </summary>
public class TicketSearchGrpcServiceImpl : TicketSearchGrpcService.TicketSearchGrpcServiceBase
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ILogger<TicketSearchGrpcServiceImpl> _logger;

    public TicketSearchGrpcServiceImpl(
        ITicketRepository ticketRepository,
        ILogger<TicketSearchGrpcServiceImpl> logger)
    {
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    public override async Task<SearchTicketResponse> SearchTicket(
        SearchTicketRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.LotteryId, out var lotteryId))
        {
            _logger.LogWarning("Invalid lottery GUID received: {LotteryId}", request.LotteryId);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid lottery_id format"));
        }

        var ticket = await _ticketRepository.GetByLotteryNumberSeriesAsync(lotteryId, request.Number, request.Series);

        if (ticket is null)
        {
            _logger.LogDebug("Ticket not found for Lottery {LotteryId}, Number {Number}, Series {Series}",
                lotteryId, request.Number, request.Series);
            return new SearchTicketResponse { Found = false };
        }

        _logger.LogInformation("Ticket found: {TicketGuid} for Lottery {LotteryId}", ticket.TicketGuid, lotteryId);

        return new SearchTicketResponse
        {
            Found = true,
            TicketGuid = ticket.TicketGuid.ToString(),
            UserId = ticket.UserId,
            Status = ticket.Status.ToString(),
            PurchaseAmount = ticket.PurchaseAmount.ToString("F2"),
            PurchaseDate = ticket.PurchaseDate.ToString("O"),
            TransactionId = ticket.TransactionId ?? string.Empty
        };
    }
}
