using CryptoJackpot.Wallet.Domain.Enums;

namespace CryptoJackpot.Wallet.Application.DTOs;

public class WalletTransactionDto
{
    public Guid TransactionGuid { get; set; }
    public Guid UserGuid { get; set; }
    public decimal Amount { get; set; }
    public WalletTransactionDirection Direction { get; set; }
    public WalletTransactionType Type { get; set; }
    public WalletTransactionStatus Status { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Administrative transaction view enriched with identity data.
/// Identity fields are intentionally excluded from the customer transaction endpoint.
/// </summary>
public class AdminWalletTransactionDto : WalletTransactionDto
{
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
}
