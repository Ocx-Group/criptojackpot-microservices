using CryptoJackpot.Domain.Core.Responses;

namespace CryptoJackpot.Order.Domain.Interfaces;

public interface ICoinPaymentProvider
{
    Task<RestResponse> CreateInvoiceAsync(
        decimal amount,
        string currency,
        List<InvoiceLineItem> items,
        string? description = null,
        string? notificationsUrl = null,
        CancellationToken cancellationToken = default);

    Task<RestResponse> GetInvoiceAsync(
        string invoiceId,
        CancellationToken cancellationToken = default);

    Task<RestResponse> GetCurrenciesAsync(CancellationToken cancellationToken = default);
}

public class InvoiceLineItem
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal Amount { get; set; }
}
