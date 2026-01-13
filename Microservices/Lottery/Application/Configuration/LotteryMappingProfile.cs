using AutoMapper;
using CryptoJackpot.Domain.Core.Models;
using CryptoJackpot.Domain.Core.Requests;
using CryptoJackpot.Lottery.Application.Commands;
using CryptoJackpot.Lottery.Application.DTOs;
using CryptoJackpot.Lottery.Application.Requests;
using CryptoJackpot.Lottery.Domain.Models;

namespace CryptoJackpot.Lottery.Application.Configuration;

public class LotteryMappingProfile : Profile
{
    public LotteryMappingProfile()
    {
        // Prize mappings
        CreateMap<Prize, PrizeDto>();
        CreateMap<PrizeImage, PrizeImageDto>();
        
        // Request to Command mappings
        CreateMap<CreatePrizeRequest, CreatePrizeCommand>();
        CreateMap<UpdatePrizeRequest, UpdatePrizeCommand>();
        
        // Command to Entity mappings
        CreateMap<CreatePrizeCommand, Prize>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.AdditionalImages, opt => opt.MapFrom(src => 
                src.AdditionalImageUrls.Select((url, index) => new PrizeImage
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = url,
                    Caption = string.Empty,
                    DisplayOrder = index
                }).ToList()));

        // LotteryNumber mappings
        CreateMap<LotteryNumber, LotteryNumberDto>();

        // Pagination mappings
        CreateMap<PaginationRequest, Pagination>();
        
        // PagedList<Prize> to PagedList<PrizeDto> mapping
        CreateMap<PagedList<Prize>, PagedList<PrizeDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize));
    }
}


