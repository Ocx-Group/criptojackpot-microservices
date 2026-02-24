using CryptoJackpot.Wallet.Application.Responses;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Queries;

/// <summary>
/// Query to retrieve all supported cryptocurrencies from CoinPayments API v2.
/// </summary>
public record GetCoinPaymentCurrenciesQuery : IRequest<Result<List<CoinPaymentCurrencyResponse>>>;

