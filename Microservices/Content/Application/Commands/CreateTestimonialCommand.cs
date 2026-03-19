using CryptoJackpot.Content.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Content.Application.Commands;

public class CreateTestimonialCommand : IRequest<Result<TestimonialDto>>
{
    public string AuthorName { get; set; } = null!;
    public string AuthorLocation { get; set; } = null!;
    public string? AuthorImageUrl { get; set; }
    public string Text { get; set; } = null!;
    public int Rating { get; set; } = 5;
    public DateTime? Date { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
