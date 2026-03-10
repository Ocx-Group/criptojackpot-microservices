using AutoMapper;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Application.Requests;
using CryptoJackpot.Wallet.Domain.Models;

namespace CryptoJackpot.Wallet.Application.Configuration;

public class WalletMappingProfile : Profile
{
    public WalletMappingProfile()
    {
        // Request to Command mappings
        CreateMap<CreateUserCryptoWalletRequest, CreateUserCryptoWalletCommand>();

        // UserCryptoWallet mappings
        CreateMap<UserCryptoWallet, UserCryptoWalletDto>();

        // WalletTransaction mappings
        CreateMap<WalletTransaction, WalletTransactionDto>();
    }
}
