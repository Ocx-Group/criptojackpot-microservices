using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Lottery.Domain.Models;

namespace CryptoJackpot.Lottery.Application.Requests;

public class UpdateLotteryDrawRequest
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int MinNumber { get; set; }
    public int MaxNumber { get; set; }
    public decimal TicketPrice { get; set; }
    public int MaxTickets { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LotteryStatus Status { get; set; }
    public LotteryType Type { get; set; }
    public string Terms { get; set; } = null!;
    public bool HasAgeRestriction { get; set; }
    public int? MinimumAge { get; set; }
    public string CryptoCurrencyId { get; set; } = null!;
    public string CryptoCurrencySymbol { get; set; } = null!;
    public decimal ReferralCommissionPercentage { get; set; } = 1.00m;
    public List<string> RestrictedCountries { get; set; } = [];
    public Dictionary<string, LotteryTranslation>? Translations { get; set; }
    public Guid? PrizeId { get; set; }
}

