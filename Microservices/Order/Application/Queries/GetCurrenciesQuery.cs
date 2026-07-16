using CryptoJackpot.Order.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Order.Application.Queries;

public class GetCurrenciesQuery : IRequest<Result<List<CoinPaymentCurrencyDto>>>
{
}
