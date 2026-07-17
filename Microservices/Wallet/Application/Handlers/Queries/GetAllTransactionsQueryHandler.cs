using AutoMapper;
using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Handlers.Queries;

public class GetAllTransactionsQueryHandler
    : IRequestHandler<GetAllTransactionsQuery, Result<PagedList<AdminWalletTransactionDto>>>
{
    private readonly IWalletRepository _repository;
    private readonly IMapper _mapper;
    private readonly IUserVerificationGrpcClient _userVerificationClient;
    private readonly ILogger<GetAllTransactionsQueryHandler> _logger;

    public GetAllTransactionsQueryHandler(
        IWalletRepository repository,
        IMapper mapper,
        IUserVerificationGrpcClient userVerificationClient,
        ILogger<GetAllTransactionsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _userVerificationClient = userVerificationClient;
        _logger = logger;
    }

    public async Task<Result<PagedList<AdminWalletTransactionDto>>> Handle(
        GetAllTransactionsQuery request, CancellationToken cancellationToken)
    {
        var pagedTransactions = await _repository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.Type,
            cancellationToken);

        var dtoItems = _mapper.Map<List<AdminWalletTransactionDto>>(pagedTransactions.Items);
        await EnrichWithUserInfoAsync(dtoItems, cancellationToken);

        var result = new PagedList<AdminWalletTransactionDto>
        {
            Items = dtoItems,
            TotalItems = pagedTransactions.TotalItems,
            PageNumber = pagedTransactions.PageNumber,
            PageSize = pagedTransactions.PageSize,
        };

        return Result.Ok(result);
    }

    private async Task EnrichWithUserInfoAsync(
        IEnumerable<AdminWalletTransactionDto> transactions,
        CancellationToken cancellationToken)
    {
        var transactionList = transactions.ToList();
        var userGuids = transactionList.Select(transaction => transaction.UserGuid).Distinct();

        var userInfoTasks = userGuids.Select(userGuid => GetUserInfoSafelyAsync(userGuid, cancellationToken));
        var userInfoResults = await Task.WhenAll(userInfoTasks);
        var userInfoByGuid = userInfoResults
            .Where(result => result.UserInfo is not null)
            .ToDictionary(result => result.UserGuid, result => result.UserInfo!);

        foreach (var transaction in transactionList)
        {
            if (!userInfoByGuid.TryGetValue(transaction.UserGuid, out var userInfo))
            {
                continue;
            }

            var fullName = string.Join(
                " ",
                new[] { userInfo.Name, userInfo.LastName }.Where(value => !string.IsNullOrWhiteSpace(value)));

            transaction.UserName = string.IsNullOrWhiteSpace(fullName) ? null : fullName;
            transaction.UserEmail = string.IsNullOrWhiteSpace(userInfo.Email) ? null : userInfo.Email;
        }
    }

    private async Task<(Guid UserGuid, UserInfoResult? UserInfo)> GetUserInfoSafelyAsync(
        Guid userGuid,
        CancellationToken cancellationToken)
    {
        try
        {
            var userInfo = await _userVerificationClient.GetUserInfoAsync(userGuid, cancellationToken);
            return (userGuid, userInfo);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Could not enrich admin wallet transactions with identity data for user {UserGuid}",
                userGuid);
            return (userGuid, null);
        }
    }
}
