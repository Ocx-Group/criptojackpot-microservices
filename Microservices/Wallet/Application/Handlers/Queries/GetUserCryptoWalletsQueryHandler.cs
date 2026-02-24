using AutoMapper;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Application.Queries;
using CryptoJackpot.Wallet.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Handlers.Queries;

public class GetUserCryptoWalletsQueryHandler : IRequestHandler<GetUserCryptoWalletsQuery, Result<List<UserCryptoWalletDto>>>
{
    private readonly IUserCryptoWalletRepository _repository;
    private readonly IMapper _mapper;

    public GetUserCryptoWalletsQueryHandler(
        IUserCryptoWalletRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<UserCryptoWalletDto>>> Handle(GetUserCryptoWalletsQuery request, CancellationToken cancellationToken)
    {
        var wallets = await _repository.GetByUserGuidAsync(request.UserGuid, cancellationToken);
        var dtos = _mapper.Map<List<UserCryptoWalletDto>>(wallets);
        return Result.Ok(dtos);
    }
}
