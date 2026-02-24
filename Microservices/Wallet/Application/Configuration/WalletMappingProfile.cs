using AutoMapper;
using CryptoJackpot.Wallet.Application.Commands;
using CryptoJackpot.Wallet.Application.DTOs;
using CryptoJackpot.Wallet.Application.DTOs.CoinPayments;
using CryptoJackpot.Wallet.Application.Requests;
using CryptoJackpot.Wallet.Application.Responses;
using CryptoJackpot.Wallet.Domain.Models;

namespace CryptoJackpot.Wallet.Application.Configuration;

public class WalletMappingProfile : Profile
{
    public WalletMappingProfile()
    {
        // Request to Command mappings
        CreateMap<CreateCoinPaymentTransactionRequest, CreateCoinPaymentTransactionCommand>();
        CreateMap<CreateUserCryptoWalletRequest, CreateUserCryptoWalletCommand>();

        // Command to Provider Request mappings
        CreateMap<CreateCoinPaymentTransactionCommand, CreateTransactionRequest>();

        // Result to Response mappings
        CreateMap<CreateTransactionResult, CreateCoinPaymentTransactionResponse>();
        CreateMap<RateResult, CoinPaymentCurrencyResponse>()
            .ForMember(dest => dest.LogoUrl,      opt => opt.MapFrom(src => src.Logo != null ? src.Logo.ImageUrl : null))
            .ForMember(dest => dest.Capabilities, opt => opt.MapFrom(src => src.Capabilities ?? new List<string>()));

        // UserCryptoWallet mappings
        CreateMap<UserCryptoWallet, UserCryptoWalletDto>();
    }
}
