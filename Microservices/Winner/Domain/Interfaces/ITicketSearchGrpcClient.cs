namespace CryptoJackpot.Winner.Domain.Interfaces;

public interface ITicketSearchGrpcClient
{
    Task<TicketSearchResult?> SearchTicketAsync(Guid lotteryId, int number, int series, CancellationToken ct = default);
}

public class TicketSearchResult
{
    public Guid TicketGuid { get; set; }
    public long UserId { get; set; }
    public string Status { get; set; } = null!;
    public decimal PurchaseAmount { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? TransactionId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
}
