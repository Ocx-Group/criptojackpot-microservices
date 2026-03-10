using FluentResults;
using MediatR;

namespace CryptoJackpot.Notification.Application.Commands;

/// <summary>
/// Command to send a purchase confirmation email after successful payment.
/// </summary>
public class SendPurchaseConfirmationCommand : IRequest<Result<bool>>
{
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; } = null!;
    public string LotteryTitle { get; set; } = null!;
    public string LotteryNo { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public DateTime PurchaseDate { get; set; }
    public List<PurchasedTicketItemDto> Tickets { get; set; } = [];
}

public class PurchasedTicketItemDto
{
    public int Number { get; set; }
    public int Series { get; set; }
    public decimal Amount { get; set; }
    public bool IsGift { get; set; }
}

