using AutoMapper;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Domain.Interfaces;
using CryptoJackpot.Wallet.Domain.Models;
using FluentResults;
using MediatR;
using CryptoJackpot.Domain.Core.Extensions;

namespace CryptoJackpot.Wallet.Application.Handlers.Commands;

public class CreateUserCryptoWalletCommandHandler : IRequestHandler<CreateUserCryptoWalletCommand, Result<UserCryptoWalletDto>>
{
    private readonly IUserCryptoWalletRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateUserCryptoWalletCommandHandler(
        IUserCryptoWalletRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserCryptoWalletDto>> Handle(CreateUserCryptoWalletCommand request, CancellationToken cancellationToken)
    {
        var existingWallets = await _repository.GetByUserGuidAsync(request.UserGuid, cancellationToken);

        var wallet = new UserCryptoWallet
        {
            UserGuid = request.UserGuid,
            Address = request.Address,
            CurrencySymbol = request.CurrencySymbol,
            CurrencyName = request.CurrencyName,
            LogoUrl = request.LogoUrl,
            Label = request.Label,
            IsDefault = existingWallets.Count == 0,
        };

        await _repository.AddAsync(wallet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<UserCryptoWalletDto>(wallet);
        return ResultExtensions.Created(dto);
    }
}
