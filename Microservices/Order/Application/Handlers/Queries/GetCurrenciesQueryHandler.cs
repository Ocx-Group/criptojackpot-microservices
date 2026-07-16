using System.Text.Json;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Order.Application.Converters;
using CryptoJackpot.Order.Application.DTOs;
using CryptoJackpot.Order.Application.DTOs.CoinPayments;
using CryptoJackpot.Order.Application.Queries;
using CryptoJackpot.Order.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Order.Application.Handlers.Queries;

public class GetCurrenciesQueryHandler
    : IRequestHandler<GetCurrenciesQuery, Result<List<CoinPaymentCurrencyDto>>>
{
    private const string CacheKey = "coinpayments:currencies";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);


    private readonly ICoinPaymentProvider _coinPaymentProvider;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetCurrenciesQueryHandler> _logger;

    public GetCurrenciesQueryHandler(
        ICoinPaymentProvider coinPaymentProvider,
        IDistributedCache cache,
        ILogger<GetCurrenciesQueryHandler> logger)
    {
        _coinPaymentProvider = coinPaymentProvider;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<List<CoinPaymentCurrencyDto>>> Handle(
        GetCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var cached = await _cache.GetStringAsync(CacheKey, cancellationToken);
            if (cached is not null)
            {
                var cachedResult = JsonSerializer.Deserialize<List<CoinPaymentCurrencyDto>>(cached, JsonDefaults.ApiResponse);
                return Result.Ok(cachedResult!);
            }

            var response = await _coinPaymentProvider.GetCurrenciesAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("CoinPayments currencies error: {StatusCode} - {Content}",
                    response.StatusCode, response.Content);
                return Result.Fail(new ExternalServiceError("CoinPayments",
                    $"Failed to fetch currencies: {response.StatusCode}"));
            }

            var currencies = response.Deserialize<List<CurrencyResult>>(JsonDefaults.ApiResponse) ?? [];

            var result = currencies.Select(c => new CoinPaymentCurrencyDto
            {
                Id = c.Id,
                Type = c.Type,
                Symbol = c.Symbol,
                Name = c.Name,
                LogoUrl = c.Logo?.ImageUrl,
                DecimalPlaces = c.DecimalPlaces,
                Rank = c.Rank,
                Status = c.Status,
                Capabilities = c.Capabilities ?? [],
                RequiredConfirmations = c.RequiredConfirmations,
                IsEnabledForPayment = c.IsEnabledForPayment
            }).ToList();

            _logger.LogInformation("Retrieved {Count} currencies from CoinPayments", result.Count);

            var serialized = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync(
                CacheKey,
                serialized,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration },
                cancellationToken);

            return Result.Ok(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching CoinPayments currencies");
            return Result.Fail(new ExternalServiceError("CoinPayments", $"Unexpected error: {ex.Message}"));
        }
    }
}
