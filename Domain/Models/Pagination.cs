namespace CryptoJackpot.Domain.Core.Models;

/// <summary>
/// Pagination parameters for paginated queries.
/// Provides sensible defaults and enforces maximum page size limits.
/// </summary>
public class Pagination
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}

