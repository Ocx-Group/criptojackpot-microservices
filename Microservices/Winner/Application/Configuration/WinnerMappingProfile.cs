using AutoMapper;
using CryptoJackpot.Winner.Application.DTOs;
using CryptoJackpot.Winner.Domain.Models;

namespace CryptoJackpot.Winner.Application.Configuration;

public class WinnerMappingProfile : Profile
{
    public WinnerMappingProfile()
    {
        CreateMap<LotteryWinner, WinnerDto>();
    }
}

