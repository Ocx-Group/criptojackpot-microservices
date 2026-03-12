using AutoMapper;
using CryptoJackpot.Domain.Core.Bus;
using CryptoJackpot.Domain.Core.IntegrationEvents.Wallet;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Handlers.Commands;

public class ProcessWithdrawalRequestCommandHandler
    : IRequestHandler<ProcessWithdrawalRequestCommand, Result<WithdrawalRequestDto>>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IWalletService _walletService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ProcessWithdrawalRequestCommandHandler> _logger;

    public ProcessWithdrawalRequestCommandHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IWalletService walletService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEventBus eventBus,
        ILogger<ProcessWithdrawalRequestCommandHandler> logger)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _walletService = walletService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<WithdrawalRequestDto>> Handle(
        ProcessWithdrawalRequestCommand request,
        CancellationToken cancellationToken)
    {
        var withdrawalRequest = await _withdrawalRequestRepository.GetByGuidAsync(
            request.RequestGuid, cancellationToken);

        if (withdrawalRequest is null)
            return Result.Fail(new NotFoundError("Withdrawal request not found."));

        if (withdrawalRequest.Status != WithdrawalRequestStatus.Pending)
            return Result.Fail(new BadRequestError("Only pending withdrawal requests can be processed."));

        if (request.Approve)
            return await HandleApproval(withdrawalRequest, request.AdminNotes, cancellationToken);

        return await HandleRejection(withdrawalRequest, request.AdminNotes, cancellationToken);
    }

    private async Task<Result<WithdrawalRequestDto>> HandleApproval(
        Domain.Models.WithdrawalRequest withdrawalRequest,
        string? adminNotes,
        CancellationToken cancellationToken)
    {
        withdrawalRequest.Status = WithdrawalRequestStatus.Approved;
        withdrawalRequest.AdminNotes = adminNotes;
        withdrawalRequest.ProcessedAt = DateTime.UtcNow;

        _withdrawalRequestRepository.Update(withdrawalRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event for Order microservice to process the CoinPayments spend
        await _eventBus.Publish(new WithdrawalApprovedEvent
        {
            RequestGuid = withdrawalRequest.RequestGuid,
            UserGuid = withdrawalRequest.UserGuid,
            Amount = withdrawalRequest.Amount,
            WalletAddress = withdrawalRequest.WalletAddress,
            CurrencySymbol = withdrawalRequest.CurrencySymbol,
            CurrencyName = withdrawalRequest.CurrencyName,
            AdminNotes = adminNotes
        });

        _logger.LogInformation(
            "Withdrawal request {RequestGuid} approved for user {UserGuid}: ${Amount} to {Address}. Event published.",
            withdrawalRequest.RequestGuid, withdrawalRequest.UserGuid,
            withdrawalRequest.Amount, withdrawalRequest.WalletAddress);

        var dto = _mapper.Map<WithdrawalRequestDto>(withdrawalRequest);
        return Result.Ok(dto);
    }

    private async Task<Result<WithdrawalRequestDto>> HandleRejection(
        Domain.Models.WithdrawalRequest withdrawalRequest,
        string? adminNotes,
        CancellationToken cancellationToken)
    {
        withdrawalRequest.Status = WithdrawalRequestStatus.Rejected;
        withdrawalRequest.AdminNotes = adminNotes;
        withdrawalRequest.ProcessedAt = DateTime.UtcNow;

        // Refund blocked funds
        var refundResult = await _walletService.ApplyTransactionAsync(
            withdrawalRequest.UserGuid,
            withdrawalRequest.Amount,
            WalletTransactionDirection.Credit,
            WalletTransactionType.WithdrawalRefund,
            withdrawalRequest.RequestGuid,
            "Refund for rejected withdrawal request",
            cancellationToken);

        if (refundResult.IsFailed)
            return refundResult.ToResult<WithdrawalRequestDto>();

        _withdrawalRequestRepository.Update(withdrawalRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Withdrawal request {RequestGuid} rejected for user {UserGuid}: ${Amount}. Funds refunded.",
            withdrawalRequest.RequestGuid, withdrawalRequest.UserGuid, withdrawalRequest.Amount);

        var dto = _mapper.Map<WithdrawalRequestDto>(withdrawalRequest);
        return Result.Ok(dto);
    }
}
