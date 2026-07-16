using CryptoJackpot.Content.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Content.Application.Commands;

public class DeleteTestimonialCommand : IRequest<Result<TestimonialDto>>
{
    public Guid TestimonialId { get; set; }
}
