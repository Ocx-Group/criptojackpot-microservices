using AutoMapper;
using CryptoJackpot.Content.Application.Commands;
using CryptoJackpot.Content.Application.DTOs;
using CryptoJackpot.Content.Domain.Interfaces;
using CryptoJackpot.Content.Domain.Models;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Responses.Errors;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Content.Application.Handlers.Commands;

public class CreateTestimonialCommandHandler : IRequestHandler<CreateTestimonialCommand, Result<TestimonialDto>>
{
    private readonly ITestimonialRepository _testimonialRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateTestimonialCommandHandler> _logger;

    public CreateTestimonialCommandHandler(
        ITestimonialRepository testimonialRepository,
        IMapper mapper,
        ILogger<CreateTestimonialCommandHandler> logger)
    {
        _testimonialRepository = testimonialRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TestimonialDto>> Handle(CreateTestimonialCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var testimonial = _mapper.Map<Testimonial>(request);
            var created = await _testimonialRepository.CreateAsync(testimonial);

            _logger.LogInformation("Testimonial {TestimonialId} created successfully", created.Id);

            return ResultExtensions.Created(_mapper.Map<TestimonialDto>(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create testimonial for {AuthorName}", request.AuthorName);
            return Result.Fail<TestimonialDto>(new InternalServerError("Failed to create testimonial"));
        }
    }
}
