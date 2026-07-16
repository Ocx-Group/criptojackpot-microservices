using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Content.Domain.Models;

public class Testimonial : BaseEntity
{
    public Guid TestimonialGuid { get; set; } = Guid.NewGuid();
    public string AuthorName { get; set; } = null!;
    public string AuthorLocation { get; set; } = null!;
    public string? AuthorImageUrl { get; set; }
    public string Text { get; set; } = null!;
    public int Rating { get; set; } = 5;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
