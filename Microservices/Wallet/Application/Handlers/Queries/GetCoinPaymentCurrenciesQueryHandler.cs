using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Wallet.Application.DTOs.CoinPayments;
using CryptoJackpot.Wallet.Application.Extensions;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Application.Responses;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Handlers.Queries;

/// <summary>
/// Handler for retrieving all supported cryptocurrencies from the CoinPayments API v2.
/// </summary>
public class GetCoinPaymentCurrenciesQueryHandler
    : IRequestHandler<GetCoinPaymentCurrenciesQuery, Result<List<CoinPaymentCurrencyResponse>>>
{
    private readonly ICoinPaymentProvider _coinPaymentProvider;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCoinPaymentCurrenciesQueryHandler> _logger;

    public GetCoinPaymentCurrenciesQueryHandler(
        ICoinPaymentProvider coinPaymentProvider,
        IMapper mapper,
        ILogger<GetCoinPaymentCurrenciesQueryHandler> logger)
    {
        _coinPaymentProvider = coinPaymentProvider;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<CoinPaymentCurrencyResponse>>> Handle(
        GetCoinPaymentCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching supported cryptocurrencies from CoinPayments API v2");

            var response = await _coinPaymentProvider.GetCurrenciesTypedAsync(cancellationToken);

            if (response is null)
            {
                _logger.LogError("CoinPayments API returned null response for currencies");
                return Result.Fail(new ExternalServiceError("CoinPayments", "API returned null response"));
            }

            if (!response.IsSuccess)
            {
                _logger.LogError("CoinPayments API error while fetching currencies: {Error}", response.Error);
                return Result.Fail(new ExternalServiceError("CoinPayments", response.Error));
            }

            var currencies = response.Currencies ?? response.Items ?? new List<RateResult>();

            _logger.LogInformation(
                "Successfully retrieved {Count} currencies from CoinPayments",
                currencies.Count);

            var result = _mapper.Map<List<CoinPaymentCurrencyResponse>>(currencies);
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

