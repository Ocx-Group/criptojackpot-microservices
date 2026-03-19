using AutoMapper;
using CryptoJackpot.Content.Application.Commands;
using CryptoJackpot.Content.Application.DTOs;
using CryptoJackpot.Content.Application.Requests;
using CryptoJackpot.Content.Domain.Models;
using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Content.Application.Configuration;

public class ContentMappingProfile : Profile
{
    public ContentMappingProfile()
    {
        // Entity to DTO
        CreateMap<Testimonial, TestimonialDto>();

        // Request to Command
        CreateMap<CreateTestimonialRequest, CreateTestimonialCommand>();
        CreateMap<UpdateTestimonialRequest, UpdateTestimonialCommand>();

        // Command to Entity
        CreateMap<CreateTestimonialCommand, Testimonial>()
            .ForMember(dest => dest.TestimonialGuid, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date ?? DateTime.UtcNow));

        // PagedList mapping
        CreateMap<PagedList<Testimonial>, PagedList<TestimonialDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize));
    }
}
