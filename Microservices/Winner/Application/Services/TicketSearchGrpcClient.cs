using CryptoJackpot.Domain.Core.Protos;
using CryptoJackpot.Winner.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Winner.Application.Services;

/// <summary>
/// gRPC client wrapper that queries Order service for ticket verification.
/// Used to verify if a ticket was sold before determining a winner.
/// </summary>
public class TicketSearchGrpcClient : ITicketSearchGrpcClient
{
    private readonly TicketSearchGrpcService.TicketSearchGrpcServiceClient _client;
    private readonly ILogger<TicketSearchGrpcClient> _logger;

    public TicketSearchGrpcClient(
        TicketSearchGrpcService.TicketSearchGrpcServiceClient client,
        ILogger<TicketSearchGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<TicketSearchResult?> SearchTicketAsync(
        Guid lotteryId, int number, int series, CancellationToken ct = default)
    {
        _logger.LogDebug("Querying Order for ticket: Lottery {LotteryId}, Number {Number}, Series {Series}",
            lotteryId, number, series);

        var response = await _client.SearchTicketAsync(
            new SearchTicketRequest
            {
                LotteryId = lotteryId.ToString(),
                Number = number,
                Series = series
            },
            cancellationToken: ct);

        if (!response.Found)
        {
            _logger.LogDebug("Order reports ticket not found for Lottery {LotteryId}, Number {Number}, Series {Series}",
                lotteryId, number, series);
            return null;
        }

        return new TicketSearchResult
        {
            TicketGuid = Guid.Parse(response.TicketGuid),
            UserId = response.UserId,
            Status = response.Status,
            PurchaseAmount = decimal.TryParse(response.PurchaseAmount, out var amount) ? amount : 0,
            PurchaseDate = DateTime.TryParse(response.PurchaseDate, out var date) ? date : DateTime.UtcNow,
            TransactionId = string.IsNullOrEmpty(response.TransactionId) ? null : response.TransactionId,
            UserName = string.IsNullOrEmpty(response.UserName) ? null : response.UserName,
            UserEmail = string.IsNullOrEmpty(response.UserEmail) ? null : response.UserEmail
        };
    }
}
