using CryptoJackpot.Order.Application.DTOs.CoinPayments;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Commands;

public class ProcessWebhookCommand : IRequest<Result>
{
    /// <summary>
    /// The CoinPayments invoice ID (from the webhook payload top-level 'id' or 'invoice.id')
    /// </summary>
    public string InvoiceId { get; set; } = string.Empty;

    /// <summary>
    /// The webhook event type (e.g., invoicePaid, invoiceCompleted, etc.)
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// The full webhook payload for extracting payment details
    /// </summary>
    public CoinPaymentsWebhookPayload Payload { get; set; } = null!;
}

