using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Handlers.Commands;

public class DeleteUserCryptoWalletCommandHandler : IRequestHandler<DeleteUserCryptoWalletCommand, Result>
{
    private readonly IUserCryptoWalletRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCryptoWalletCommandHandler(
        IUserCryptoWalletRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteUserCryptoWalletCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _repository.GetByWalletGuidAsync(request.WalletGuid, cancellationToken);

        if (wallet is null || wallet.UserGuid != request.UserGuid)
            return Result.Fail(new NotFoundError("Wallet not found"));

        _repository.Delete(wallet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
