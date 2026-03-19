using AutoMapper;
using CryptoJackpot.Content.Application.Commands;
using CryptoJackpot.Content.Application.DTOs;
using CryptoJackpot.Content.Domain.Interfaces;
using CryptoJackpot.Domain.Core.Responses.Errors;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Content.Application.Handlers.Commands;

public class UpdateTestimonialCommandHandler : IRequestHandler<UpdateTestimonialCommand, Result<TestimonialDto>>
{
    private readonly ITestimonialRepository _testimonialRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateTestimonialCommandHandler> _logger;

    public UpdateTestimonialCommandHandler(
        ITestimonialRepository testimonialRepository,
        IMapper mapper,
        ILogger<UpdateTestimonialCommandHandler> logger)
    {
        _testimonialRepository = testimonialRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TestimonialDto>> Handle(UpdateTestimonialCommand request, CancellationToken cancellationToken)
    {
        var testimonial = await _testimonialRepository.GetByGuidAsync(request.TestimonialId);

        if (testimonial is null)
            return Result.Fail<TestimonialDto>(new NotFoundError("Testimonial not found"));

        try
        {
            testimonial.AuthorName = request.AuthorName;
            testimonial.AuthorLocation = request.AuthorLocation;
            testimonial.AuthorImageUrl = request.AuthorImageUrl;
            testimonial.Text = request.Text;
            testimonial.Rating = request.Rating;
            testimonial.IsActive = request.IsActive;
            testimonial.SortOrder = request.SortOrder;

            if (request.Date.HasValue)
                testimonial.Date = request.Date.Value;

            var updated = await _testimonialRepository.UpdateAsync(testimonial);

            _logger.LogInformation("Testimonial {TestimonialId} updated successfully", updated.Id);

            return Result.Ok(_mapper.Map<TestimonialDto>(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update testimonial {TestimonialId}", request.TestimonialId);
            return Result.Fail<TestimonialDto>(new InternalServerError("Failed to update testimonial"));
        }
    }
}
