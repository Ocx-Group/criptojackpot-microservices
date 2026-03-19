using Asp.Versioning;
using AutoMapper;
using CryptoJackpot.Content.Application.Commands;
using CryptoJackpot.Content.Application.Queries;
using CryptoJackpot.Content.Application.Requests;
using CryptoJackpot.Domain.Core.Extensions;
using CryptoJackpot.Domain.Core.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoJackpot.Content.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/testimonials")]
public class TestimonialController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public TestimonialController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all active testimonials (public endpoint for landing page)
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveTestimonials()
    {
        var query = new GetActiveTestimonialsQuery();
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get all testimonials with pagination (admin)
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllTestimonials([FromQuery] PaginationRequest pagination)
    {
        var query = new GetAllTestimonialsQuery
        {
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };

        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get a testimonial by ID (admin)
    /// </summary>
    [Authorize]
    [HttpGet("{testimonialId:guid}")]
    public async Task<IActionResult> GetTestimonialById([FromRoute] Guid testimonialId)
    {
        var query = new GetTestimonialByIdQuery { TestimonialId = testimonialId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Create a new testimonial (admin)
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateTestimonial([FromBody] CreateTestimonialRequest request)
    {
        var command = _mapper.Map<CreateTestimonialCommand>(request);
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Update an existing testimonial (admin)
    /// </summary>
    [Authorize]
    [HttpPut("{testimonialId:guid}")]
    public async Task<IActionResult> UpdateTestimonial([FromRoute] Guid testimonialId, [FromBody] UpdateTestimonialRequest request)
    {
        var command = _mapper.Map<UpdateTestimonialCommand>(request);
        command.TestimonialId = testimonialId;

        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Delete a testimonial (admin, soft delete)
    /// </summary>
    [Authorize]
    [HttpDelete("{testimonialId:guid}")]
    public async Task<IActionResult> DeleteTestimonial([FromRoute] Guid testimonialId)
    {
        var command = new DeleteTestimonialCommand { TestimonialId = testimonialId };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}
