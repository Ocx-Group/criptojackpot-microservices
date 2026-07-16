using CryptoJackpot.Order.Application.DTOs.CoinPayments;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Commands;

/// <summary>
/// Command to register a webhook on CoinPayments for receiving invoice notifications.
/// This calls POST /merchant/clients/:id/webhooks on CoinPayments API.
/// </summary>
public class RegisterWebhookCommand : IRequest<Result<RegisterWebhookResult>>
{
    /// <summary>
    /// The public URL where CoinPayments will POST webhook notifications.
    /// If null, will be read from configuration (CoinPayments:WebhookNotificationsUrl).
    /// </summary>
    public string? NotificationsUrl { get; set; }

    /// <summary>
    /// The event types to subscribe to.
    /// If null/empty, defaults to all supported events.
    /// </summary>
    public List<string>? Notifications { get; set; }
}

