using CryptoJackpot.Content.Application.DTOs;
using CryptoJackpot.Domain.Core.Models;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Content.Application.Queries;

public class GetAllTestimonialsQuery : IRequest<Result<PagedList<TestimonialDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
