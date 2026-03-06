namespace CryptoJackpot.Order.Application.DTOs;

public class PayOrderResponse
{
    public Guid OrderId { get; set; }
    public string InvoiceId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string StatusUrl { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int SecondsRemaining { get; set; }
}
