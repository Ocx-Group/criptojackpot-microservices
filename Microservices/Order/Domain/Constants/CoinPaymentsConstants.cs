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
}

[SuppressMessage("Design", "S1075:URIs should not be hardcoded")]
public static class CoinPaymentsDefaults
{
    public const string BaseUrl = "https://a-api.coinpayments.net/api/";
    public const int HttpClientTimeoutSeconds = 30;
    public const string HttpClientName = "CoinPayments";
    public const string DefaultInvoiceCurrency = "5057";
}

public static class CoinPaymentsEndpoints
{
    public const string CreateInvoice = "v2/merchant/invoices";
    public const string GetInvoiceById = "v2/merchant/invoices/{0}";
    public const string GetCurrencies = "v2/currencies";
}

public static class CoinPaymentsResilience
{
    public const int RetryCount = 3;
    public const int CircuitBreakerFailureThreshold = 5;
    public const int CircuitBreakerDurationSeconds = 30;
}
