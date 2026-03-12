using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace CryptoJackpot.Order.Domain.Constants;

public static class CoinPaymentsConfigKeys
{
    public const string Section = "CoinPayments";
    public const string ClientSecret = "CoinPayments:ClientSecret";
    public const string ClientId = "CoinPayments:ClientId";
    public const string BaseUrl = "CoinPayments:BaseUrl";
    public const string InvoiceCurrency = "CoinPayments:InvoiceCurrency";
    public const string WebhookNotificationsUrl = "CoinPayments:WebhookNotificationsUrl";
    public const string WebhookSecret = "CoinPayments:WebhookSecret";
}

/// <summary>
/// CoinPayments webhook event types. Use case-insensitive comparison as recommended by CoinPayments.
/// </summary>
public static class CoinPaymentsWebhookEvents
{
    public const string InvoiceCreated = "invoiceCreated";
    public const string InvoicePending = "invoicePending";
    public const string InvoicePaid = "invoicePaid";
    public const string InvoiceCompleted = "invoiceCompleted";
    public const string InvoiceCancelled = "invoiceCancelled";
    public const string InvoiceTimedOut = "invoiceTimedOut";
    public const string InvoicePaymentCreated = "invoicePaymentCreated";
    public const string InvoicePaymentTimedOut = "invoicePaymentTimedOut";

    public static readonly FrozenSet<string> All = new[]
    {
        InvoiceCreated,
        InvoicePending,
        InvoicePaid,
        InvoiceCompleted,
        InvoiceCancelled,
        InvoiceTimedOut,
        InvoicePaymentCreated,
        InvoicePaymentTimedOut
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
}

[SuppressMessage("Design", "S1075:URIs should not be hardcoded")]
public static class CoinPaymentsDefaults
{
    public const string BaseUrl = "https://a-api.coinpayments.net/api/";
    public const int HttpClientTimeoutSeconds = 30;
    public const string HttpClientName = "CoinPayments";
    public const string DefaultInvoiceCurrency = "5057";
    public const string SuccessUrl = "https://criptojackpot.com/my-tickets"; 
    public const string CancelUrl = "https://criptojackpot.com/";
}

public static class CoinPaymentsEndpoints
{
    public const string CreateInvoice = "v2/merchant/invoices";
    public const string GetInvoiceById = "v2/merchant/invoices/{0}";
    public const string GetCurrencies = "v2/currencies";
    public const string RegisterWebhook = "v2/merchant/clients/{0}/webhooks";
    public const string CreateSpendRequest = "v2/merchant/wallets/{0}/spend/request";
    public const string ConfirmSpendRequest = "v2/merchant/wallets/{0}/spend/confirmation";
    public const string GetMerchantWallets = "v2/merchant/wallets";
}

public static class CoinPaymentsResilience
{
    public const int RetryCount = 3;
    public const int CircuitBreakerFailureThreshold = 5;
    public const int CircuitBreakerDurationSeconds = 30;
}
