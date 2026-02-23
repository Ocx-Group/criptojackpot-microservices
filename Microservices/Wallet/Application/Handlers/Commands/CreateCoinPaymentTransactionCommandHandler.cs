using AutoMapper;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.DTOs.CoinPayments;
using CryptoJackpot.Wallet.Application.Extensions;
using CryptoJackpot.Wallet.Application.Responses;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Handlers.Commands;

/// <summary>
/// Handler for creating cryptocurrency payment transactions via CoinPayments
/// </summary>
public class CreateCoinPaymentTransactionCommandHandler 
    : IRequestHandler<CreateCoinPaymentTransactionCommand, Result<CreateCoinPaymentTransactionResponse>>
{
    private readonly ICoinPaymentProvider _coinPaymentProvider;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCoinPaymentTransactionCommandHandler> _logger;

    public CreateCoinPaymentTransactionCommandHandler(
        ICoinPaymentProvider coinPaymentProvider,
        IMapper mapper,
        ILogger<CreateCoinPaymentTransactionCommandHandler> logger)
    {
        _coinPaymentProvider = coinPaymentProvider;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CreateCoinPaymentTransactionResponse>> Handle(
        CreateCoinPaymentTransactionCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Creating CoinPayment transaction: {Amount} {CurrencyFrom} -> {CurrencyTo}",
                request.Amount, request.CurrencyFrom, request.CurrencyTo);

            var createTransactionRequest = _mapper.Map<CreateTransactionRequest>(request);

            var response = await _coinPaymentProvider.CreateTransactionAsync(
                createTransactionRequest, 
                cancellationToken);

            if (response is null)
            {
                _logger.LogError("CoinPayments API returned null response");
                return Result.Fail(new ExternalServiceError("CoinPayments", "API returned null response"));
            }

            if (!response.IsSuccess)
            {
                _logger.LogError("CoinPayments API error: {Error}", response.Error);
                return Result.Fail(new ExternalServiceError("CoinPayments", response.Error));
            }

            // API v2 returns "invoices" array — use FirstResult to get the created invoice
            var invoice = response.FirstResult;
            if (invoice is null)
            {
                _logger.LogError("CoinPayments API returned success but no invoice in response");
                return Result.Fail(new ExternalServiceError("CoinPayments", "No invoice returned in response"));
            }

            _logger.LogInformation(
                "CoinPayment transaction created successfully. TxId: {TransactionId}",
                invoice.TransactionId);

            return ResultExtensions.Created(_mapper.Map<CreateCoinPaymentTransactionResponse>(invoice));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating CoinPayment transaction");
            return Result.Fail(new ExternalServiceError("CoinPayments", $"Unexpected error: {ex.Message}"));
        }
    }
}
