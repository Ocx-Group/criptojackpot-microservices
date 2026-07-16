namespace CryptoJackpot.Identity.Application.DTOs;

public class WishListItemDto
{
    public Guid WishListItemGuid { get; set; }
    public Guid LotteryGuid { get; set; }
    public DateTime CreatedAt { get; set; }
}
