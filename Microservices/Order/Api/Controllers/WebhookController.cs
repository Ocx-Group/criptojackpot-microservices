using System.Text.Json;
using Asp.Versioning;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Order.Api.Filters;
using CryptoJackpot.Order.Application.Commands;
using CryptoJackpot.Order.Application.Converters;
using CryptoJackpot.Order.Application.DTOs.CoinPayments.Webhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Order.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IMediator mediator, ILogger<WebhookController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Registers a webhook on CoinPayments to receive invoice notifications.
    /// Calls POST /merchant/clients/:id/webhooks on CoinPayments API.
    /// Requires admin authentication.
    /// </summary>
    [HttpPost("coinpayments/register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterWebhook([FromBody] RegisterWebhookCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Receives CoinPayments webhook notifications for invoice events.
    /// Validates the request signature via the CoinPaymentsWebhookSignatureFilter.
    /// Always returns 200 OK to CoinPayments to acknowledge receipt.
    /// </summary>
    [HttpPost("coinpayments")]
    [AllowAnonymous]
    [ServiceFilter(typeof(CoinPaymentsWebhookSignatureFilter))]
    public async Task<IActionResult> CoinPaymentsWebhook()
    {
        string body;
        try
        {
            // Read the raw body (already buffered by the signature filter)
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body);
            body = await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read CoinPayments webhook request body");
            // Return 200 to prevent retries for unreadable requests
            return Ok(new { success = false, message = "Failed to read request body" });
        }

        CoinPaymentsWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<CoinPaymentsWebhookPayload>(body, JsonDefaults.Webhook);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize CoinPayments webhook payload: {Body}", body);
            // Return 200 to prevent retries for malformed payloads
            return Ok(new { success = false, message = "Invalid webhook payload format" });
        }

        if (payload is null)
        {
            _logger.LogWarning("CoinPayments webhook payload deserialized to null: {Body}", body);
            return Ok(new { success = false, message = "Empty webhook payload" });
        }

        _logger.LogInformation(
            "CoinPayments webhook received. Type: {Type}, InvoiceId: {InvoiceId}",
            payload.Type, payload.Invoice?.Id ?? payload.Id);

        var command = new ProcessWebhookCommand
        {
            InvoiceId = payload.Invoice?.Id ?? payload.Id,
            EventType = payload.Type,
            Payload = payload
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            _logger.LogWarning(
                "CoinPayments webhook processing failed. Type: {Type}, InvoiceId: {InvoiceId}, Error: {Error}",
                payload.Type, payload.Invoice?.Id ?? payload.Id,
                result.Errors.FirstOrDefault()?.Message);
        }

        // Always return 200 OK to CoinPayments to acknowledge receipt
        // This prevents CoinPayments from retrying the webhook unnecessarily
        return Ok(new { success = result.IsSuccess });
    }
}

