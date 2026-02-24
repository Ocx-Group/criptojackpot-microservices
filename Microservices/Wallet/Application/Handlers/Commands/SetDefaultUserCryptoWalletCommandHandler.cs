using AutoMapper;
using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Handlers.Commands;

public class SetDefaultUserCryptoWalletCommandHandler : IRequestHandler<SetDefaultUserCryptoWalletCommand, Result<UserCryptoWalletDto>>
{
    private readonly IUserCryptoWalletRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SetDefaultUserCryptoWalletCommandHandler(
        IUserCryptoWalletRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserCryptoWalletDto>> Handle(SetDefaultUserCryptoWalletCommand request, CancellationToken cancellationToken)
    {
        var wallets = await _repository.GetByUserGuidAsync(request.UserGuid, cancellationToken);

        var targetWallet = wallets.FirstOrDefault(w => w.WalletGuid == request.WalletGuid);
        if (targetWallet is null)
            return Result.Fail(new NotFoundError("Wallet not found"));

        foreach (var wallet in wallets)
        {
            wallet.IsDefault = wallet.WalletGuid == request.WalletGuid;
            wallet.UpdatedAt = DateTime.UtcNow;
            _repository.Update(wallet);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<UserCryptoWalletDto>(targetWallet);
        return Result.Ok(dto);
    }
}
