using CryptoJackpot.Content.Domain.Models;
using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Content.Domain.Interfaces;

public interface ITestimonialRepository
{
    Task<Testimonial> CreateAsync(Testimonial testimonial);
    Task<Testimonial?> GetByGuidAsync(Guid testimonialGuid);
    Task<PagedList<Testimonial>> GetAllAsync(Pagination pagination);
    Task<List<Testimonial>> GetAllActiveAsync();
    Task<Testimonial> UpdateAsync(Testimonial testimonial);
    Task<Testimonial> DeleteAsync(Testimonial testimonial);
}
