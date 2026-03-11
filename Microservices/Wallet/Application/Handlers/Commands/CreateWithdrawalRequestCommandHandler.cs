using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Domain.Enums;
using CryptoJackpot.Wallet.Domain.Interfaces;
using CryptoJackpot.Wallet.Domain.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Wallet.Application.Handlers.Commands;

public class CreateWithdrawalRequestCommandHandler
    : IRequestHandler<CreateWithdrawalRequestCommand, Result<WithdrawalRequestDto>>
{
    private const string CacheKeyPrefix = "withdrawal-code:";
    private const decimal MinWithdrawalAmount = 10.00m;

    private readonly IUserVerificationGrpcClient _userVerificationClient;
    private readonly IUserCryptoWalletRepository _cryptoWalletRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IWalletService _walletService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateWithdrawalRequestCommandHandler> _logger;

    public CreateWithdrawalRequestCommandHandler(
        IUserVerificationGrpcClient userVerificationClient,
        IUserCryptoWalletRepository cryptoWalletRepository,
        IWithdrawalRequestRepository withdrawalRequestRepository,
        IWalletService walletService,
        IUnitOfWork unitOfWork,
        IDistributedCache cache,
        IMapper mapper,
        ILogger<CreateWithdrawalRequestCommandHandler> logger)
    {
        _userVerificationClient = userVerificationClient;
        _cryptoWalletRepository = cryptoWalletRepository;
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _walletService = walletService;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<WithdrawalRequestDto>> Handle(
        CreateWithdrawalRequestCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate amount
        if (request.Amount < MinWithdrawalAmount)
            return Result.Fail(new BadRequestError($"Minimum withdrawal amount is ${MinWithdrawalAmount:F2}."));

        // 2. Check for existing pending withdrawal
        if (await _withdrawalRequestRepository.HasPendingRequestAsync(request.UserGuid, cancellationToken))
            return Result.Fail(new BadRequestError("You already have a pending withdrawal request."));

        // 3. Get user's crypto wallet
        var cryptoWallet = await _cryptoWalletRepository.GetByWalletGuidAsync(request.WalletGuid, cancellationToken);
        if (cryptoWallet is null || cryptoWallet.UserGuid != request.UserGuid)
            return Result.Fail(new NotFoundError("Crypto wallet not found."));

        // 4. Verify identity (2FA or email code)
        var verificationResult = await VerifyIdentityAsync(request, cancellationToken);
        if (verificationResult.IsFailed)
            return verificationResult.ToResult<WithdrawalRequestDto>();

        // 5. Create withdrawal request
        var withdrawalRequest = new WithdrawalRequest
        {
            UserGuid = request.UserGuid,
            Amount = request.Amount,
            WalletAddress = cryptoWallet.Address,
            CurrencySymbol = cryptoWallet.CurrencySymbol,
            CurrencyName = cryptoWallet.CurrencyName,
        };

        // 6. Block funds: create a Pending debit transaction
        var transactionResult = await _walletService.ApplyTransactionAsync(
            request.UserGuid,
            request.Amount,
            WalletTransactionDirection.Debit,
            WalletTransactionType.Withdrawal,
            withdrawalRequest.RequestGuid,
            $"Withdrawal request to {cryptoWallet.CurrencySymbol} wallet",
            cancellationToken);

        if (transactionResult.IsFailed)
            return transactionResult.ToResult<WithdrawalRequestDto>();

        // 7. Save withdrawal request
        await _withdrawalRequestRepository.AddAsync(withdrawalRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Invalidate the email verification code after successful use
        if (!string.IsNullOrWhiteSpace(request.EmailVerificationCode))
        {
            var cacheKey = $"{CacheKeyPrefix}{request.UserGuid}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);
        }

        _logger.LogInformation(
            "Withdrawal request {RequestGuid} created for user {UserGuid}: ${Amount} to {Wallet}",
            withdrawalRequest.RequestGuid, request.UserGuid, request.Amount, cryptoWallet.Address);

        var dto = _mapper.Map<WithdrawalRequestDto>(withdrawalRequest);
        return Result.Ok(dto);
    }

    private async Task<Result> VerifyIdentityAsync(
        CreateWithdrawalRequestCommand request,
        CancellationToken cancellationToken)
    {
        var userInfo = await _userVerificationClient.GetUserInfoAsync(request.UserGuid, cancellationToken);
        if (userInfo is null)
            return Result.Fail(new NotFoundError("User not found."));

        if (userInfo.TwoFactorEnabled)
        {
            // Verify TOTP code via Identity gRPC
            if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
                return Result.Fail(new BadRequestError("Two-factor authentication code is required."));

            var isValid = await _userVerificationClient.VerifyTotpCodeAsync(
                request.UserGuid, request.TwoFactorCode, cancellationToken);

            if (!isValid)
                return Result.Fail(new UnauthorizedError("Invalid two-factor authentication code."));
        }
        else
        {
            // Verify email code from distributed cache
            if (string.IsNullOrWhiteSpace(request.EmailVerificationCode))
                return Result.Fail(new BadRequestError("Email verification code is required."));

            var cacheKey = $"{CacheKeyPrefix}{request.UserGuid}";
            var storedHash = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (storedHash is null)
                return Result.Fail(new BadRequestError("Verification code expired. Please request a new one."));

            var providedHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(request.EmailVerificationCode)));

            if (storedHash != providedHash)
                return Result.Fail(new UnauthorizedError("Invalid verification code."));
        }

        return Result.Ok();
    }
}
