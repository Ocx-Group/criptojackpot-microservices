using AutoMapper;
using CryptoJackpot.Content.Application.Commands;
using CryptoJackpot.Content.Application.DTOs;
using CryptoJackpot.Content.Domain.Interfaces;
using CryptoJackpot.Domain.Core.Responses.Errors;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CryptoJackpot.Content.Application.Handlers.Commands;

public class DeleteTestimonialCommandHandler : IRequestHandler<DeleteTestimonialCommand, Result<TestimonialDto>>
{
    private readonly ITestimonialRepository _testimonialRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<DeleteTestimonialCommandHandler> _logger;

    public DeleteTestimonialCommandHandler(
        ITestimonialRepository testimonialRepository,
        IMapper mapper,
        ILogger<DeleteTestimonialCommandHandler> logger)
    {
        _testimonialRepository = testimonialRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TestimonialDto>> Handle(DeleteTestimonialCommand request, CancellationToken cancellationToken)
    {
        var testimonial = await _testimonialRepository.GetByGuidAsync(request.TestimonialId);

        if (testimonial is null)
            return Result.Fail<TestimonialDto>(new NotFoundError("Testimonial not found"));

        try
        {
            var deleted = await _testimonialRepository.DeleteAsync(testimonial);

            _logger.LogInformation("Testimonial {TestimonialId} deleted successfully", deleted.Id);

            return Result.Ok(_mapper.Map<TestimonialDto>(deleted));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete testimonial {TestimonialId}", request.TestimonialId);
            return Result.Fail<TestimonialDto>(new InternalServerError("Failed to delete testimonial"));
        }
    }
}
