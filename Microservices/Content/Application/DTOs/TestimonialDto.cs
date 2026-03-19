namespace CryptoJackpot.Content.Application.DTOs;

public class TestimonialDto
{
    public Guid TestimonialGuid { get; set; }
    public string AuthorName { get; set; } = null!;
    public string AuthorLocation { get; set; } = null!;
    public string? AuthorImageUrl { get; set; }
    public string Text { get; set; } = null!;
    public int Rating { get; set; }
    public DateTime Date { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
