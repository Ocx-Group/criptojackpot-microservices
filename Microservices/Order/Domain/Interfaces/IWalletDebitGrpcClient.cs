namespace CryptoJackpot.Order.Domain.Interfaces;

/// <summary>
/// Client interface for debiting internal wallet balance via gRPC.
/// Wallet microservice is the single source of truth for balance operations.
/// </summary>
public interface IWalletDebitGrpcClient
{
    /// <summary>
    /// Debits the user's internal wallet balance for a ticket purchase.
    /// Returns the transaction GUID on success, or throws on failure.
    /// </summary>
    Task<WalletDebitResult> DebitBalanceAsync(
        Guid userGuid,
        decimal amount,
        Guid orderId,
        string description,
        CancellationToken cancellationToken = default);
}

public class WalletDebitResult
{
    public bool Success { get; set; }
    public string? TransactionGuid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal BalanceAfter { get; set; }
}
