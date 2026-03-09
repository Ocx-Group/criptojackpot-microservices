using System.Text.Json.Serialization;

namespace CryptoJackpot.Order.Application.DTOs.CoinPayments.Webhook;

public class WebhookHotWallet
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("paymentId")]
    public string? PaymentId { get; set; }

    [JsonPropertyName("currency")]
    public WebhookCurrencyRef? Currency { get; set; }

    [JsonPropertyName("paymentReceiveAddress")]
    public string? PaymentReceiveAddress { get; set; }

    [JsonPropertyName("paymentSubTotal")]
    public long? PaymentSubTotal { get; set; }

    [JsonPropertyName("confirmedAmount")]
    public long? ConfirmedAmount { get; set; }

    [JsonPropertyName("confirmations")]
    public int? Confirmations { get; set; }

    [JsonPropertyName("requiredConfirmations")]
    public int? RequiredConfirmations { get; set; }

    [JsonPropertyName("buyerDepositTxHashes")]
    public List<string>? BuyerDepositTxHashes { get; set; }

    [JsonPropertyName("buyerDepositTxIds")]
    public List<string>? BuyerDepositTxIds { get; set; }
}

