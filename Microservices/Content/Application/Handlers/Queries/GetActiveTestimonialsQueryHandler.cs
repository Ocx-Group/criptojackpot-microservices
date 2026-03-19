using AutoMapper;
using CryptoJackpot.Content.Application.DTOs;
using CryptoJackpot.Content.Application.Queries;
using CryptoJackpot.Content.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Content.Application.Handlers.Queries;

public class GetActiveTestimonialsQueryHandler : IRequestHandler<GetActiveTestimonialsQuery, Result<List<TestimonialDto>>>
{
    private readonly ITestimonialRepository _testimonialRepository;
    private readonly IMapper _mapper;

    public GetActiveTestimonialsQueryHandler(
        ITestimonialRepository testimonialRepository,
        IMapper mapper)
    {
        _testimonialRepository = testimonialRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<TestimonialDto>>> Handle(GetActiveTestimonialsQuery request, CancellationToken cancellationToken)
    {
        var testimonials = await _testimonialRepository.GetAllActiveAsync();
        var result = _mapper.Map<List<TestimonialDto>>(testimonials);

        return Result.Ok(result);
    }
}
