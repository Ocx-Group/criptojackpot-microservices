using CryptoJackpot.Content.Data.Context;
using CryptoJackpot.Content.Domain.Interfaces;
using CryptoJackpot.Content.Domain.Models;
using CryptoJackpot.Domain.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoJackpot.Content.Data.Repositories;

public class TestimonialRepository : ITestimonialRepository
{
    private readonly ContentDbContext _context;

    public TestimonialRepository(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<Testimonial> CreateAsync(Testimonial testimonial)
    {
        var now = DateTime.UtcNow;
        testimonial.CreatedAt = now;
        testimonial.UpdatedAt = now;

        await _context.Testimonials.AddAsync(testimonial);
        await _context.SaveChangesAsync();
        return testimonial;
    }

    public async Task<Testimonial?> GetByGuidAsync(Guid testimonialGuid)
        => await _context.Testimonials
            .FirstOrDefaultAsync(t => t.TestimonialGuid == testimonialGuid && t.DeletedAt == null);

    public async Task<PagedList<Testimonial>> GetAllAsync(Pagination pagination)
    {
        var query = _context.Testimonials.Where(t => t.DeletedAt == null);
        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedList<Testimonial>
        {
            Items = items,
            TotalItems = totalItems,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<List<Testimonial>> GetAllActiveAsync()
        => await _context.Testimonials
            .Where(t => t.IsActive && t.DeletedAt == null)
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<Testimonial> UpdateAsync(Testimonial testimonial)
    {
        testimonial.UpdatedAt = DateTime.UtcNow;
        _context.Testimonials.Update(testimonial);
        await _context.SaveChangesAsync();
        return testimonial;
    }

    public async Task<Testimonial> DeleteAsync(Testimonial testimonial)
    {
        testimonial.UpdatedAt = DateTime.UtcNow;
        testimonial.DeletedAt = DateTime.UtcNow;
        _context.Testimonials.Update(testimonial);
        await _context.SaveChangesAsync();
        return testimonial;
    }
}
