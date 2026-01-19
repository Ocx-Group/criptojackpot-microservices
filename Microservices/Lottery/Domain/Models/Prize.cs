using CryptoJackpot.Lottery.Domain.Enums;
using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Lottery.Domain.Models;

public class Prize : BaseEntity
{
    public Guid PrizeGuid { get; set; } = Guid.NewGuid();
    public long? LotteryId { get; set; }
    public int Tier { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal EstimatedValue { get; set; }
    public PrizeType Type { get; set; }
    public string MainImageUrl { get; set; } = null!;
    public List<PrizeImage> AdditionalImages { get; set; } = null!;
    public Dictionary<string, string> Specifications { get; set; } = null!;
    public decimal? CashAlternative { get; set; }
    public bool IsDeliverable { get; set; }
    public bool IsDigital { get; set; }

    // Referencia al ticket ganador (microservicio Order)
    public Guid? WinnerTicketId { get; set; }
    public DateTime? ClaimedAt { get; set; }

    // Navegación interna del microservicio Lottery
    public virtual LotteryDraw Lottery { get; set; } = null!;
}