using AutoMapper;
using CryptoJackpot.Content.Application.DTOs;
using CryptoJackpot.Content.Application.Queries;
using CryptoJackpot.Content.Domain.Interfaces;
using CryptoJackpot.Domain.Core.Responses.Errors;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Content.Application.Handlers.Queries;

public class GetTestimonialByIdQueryHandler : IRequestHandler<GetTestimonialByIdQuery, Result<TestimonialDto>>
{
    private readonly ITestimonialRepository _testimonialRepository;
    private readonly IMapper _mapper;

    public GetTestimonialByIdQueryHandler(
        ITestimonialRepository testimonialRepository,
        IMapper mapper)
    {
        _testimonialRepository = testimonialRepository;
        _mapper = mapper;
    }

    public async Task<Result<TestimonialDto>> Handle(GetTestimonialByIdQuery request, CancellationToken cancellationToken)
    {
        var testimonial = await _testimonialRepository.GetByGuidAsync(request.TestimonialId);

        if (testimonial is null)
            return Result.Fail<TestimonialDto>(new NotFoundError("Testimonial not found"));

        return Result.Ok(_mapper.Map<TestimonialDto>(testimonial));
    }
}
