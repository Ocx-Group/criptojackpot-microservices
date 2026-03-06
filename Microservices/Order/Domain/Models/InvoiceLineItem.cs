namespace CryptoJackpot.Order.Domain.Models;

public class InvoiceLineItem
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal Amount { get; set; }
}

