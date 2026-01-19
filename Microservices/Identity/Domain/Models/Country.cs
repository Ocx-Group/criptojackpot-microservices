using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Identity.Domain.Models;

public class Country : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Iso3 { get; set; }
    public string? NumericCode { get; set; }
    public string? Iso2 { get; set; }
    public string? PhoneCode { get; set; }
    public string? Capital { get; set; }
    public string? Currency { get; set; }
    public string? CurrencyName { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? Tld { get; set; }
    public string? Native { get; set; }
    public string? Region { get; set; }
    public string? Subregion { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public ICollection<User> Users { get; set; } = null!;
}