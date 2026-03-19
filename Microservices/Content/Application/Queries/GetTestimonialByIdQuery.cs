using CryptoJackpot.Content.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Content.Application.Queries;

public class GetTestimonialByIdQuery : IRequest<Result<TestimonialDto>>
{
    public Guid TestimonialId { get; set; }
}
