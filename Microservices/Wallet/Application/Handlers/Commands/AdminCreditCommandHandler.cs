using AutoMapper;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Handlers.Commands;

public class AdminCreditCommandHandler
    : IRequestHandler<AdminCreditCommand, Result<WalletTransactionDto>>
{
    private readonly IWalletService _walletService;
    private readonly IMapper _mapper;
    private readonly ILogger<AdminCreditCommandHandler> _logger;

    public AdminCreditCommandHandler(
        IWalletService walletService,
        IMapper mapper,
        ILogger<AdminCreditCommandHandler> logger)
    {
        _walletService = walletService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<WalletTransactionDto>> Handle(
        AdminCreditCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _walletService.ApplyTransactionAsync(
            request.UserGuid,
            request.Amount,
            WalletTransactionDirection.Credit,
            WalletTransactionType.AdminCredit,
            description: request.Description,
            cancellationToken: cancellationToken);

        if (result.IsFailed)
            return result.ToResult<WalletTransactionDto>();

        _logger.LogInformation(
            "Admin credit of ${Amount} applied to user {UserGuid}. Description: {Description}",
            request.Amount, request.UserGuid, request.Description);

        var dto = _mapper.Map<WalletTransactionDto>(result.Value);
        return Result.Ok(dto);
    }
}
