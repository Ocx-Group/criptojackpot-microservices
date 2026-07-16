using CryptoJackpot.Content.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Content.Application.Queries;

public class GetActiveTestimonialsQuery : IRequest<Result<List<TestimonialDto>>>
{
}
