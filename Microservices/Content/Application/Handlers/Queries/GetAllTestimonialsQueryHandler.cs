using AutoMapper;
using CryptoJackpot.Content.Application.DTOs;
using CryptoJackpot.Content.Application.Queries;
using CryptoJackpot.Content.Domain.Interfaces;
using CryptoJackpot.Domain.Core.Models;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Content.Application.Handlers.Queries;

public class GetAllTestimonialsQueryHandler : IRequestHandler<GetAllTestimonialsQuery, Result<PagedList<TestimonialDto>>>
{
    private readonly ITestimonialRepository _testimonialRepository;
    private readonly IMapper _mapper;

    public GetAllTestimonialsQueryHandler(
        ITestimonialRepository testimonialRepository,
        IMapper mapper)
    {
        _testimonialRepository = testimonialRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<TestimonialDto>>> Handle(GetAllTestimonialsQuery request, CancellationToken cancellationToken)
    {
        var pagination = new Pagination
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var pagedTestimonials = await _testimonialRepository.GetAllAsync(pagination);
        var result = _mapper.Map<PagedList<TestimonialDto>>(pagedTestimonials);

        return Result.Ok(result);
    }
}
