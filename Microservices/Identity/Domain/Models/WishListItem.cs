using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Identity.Domain.Models;

public class WishListItem : BaseEntity
{
    public Guid WishListItemGuid { get; set; } = Guid.NewGuid();
    public long UserId { get; set; }
    public Guid UserGuid { get; set; }
    public Guid LotteryGuid { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
