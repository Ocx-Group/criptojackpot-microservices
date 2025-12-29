using CryptoJackpot.Domain.Core.Models;
namespace CryptoJackpot.Lottery.Domain.Models;

public class LotteryNumber : BaseEntity
{
    public Guid Id { get; set; }
    public Guid LotteryId { get; set; }
    public int Number { get; set; }
    public int Series { get; set; }
    public bool IsAvailable { get; set; }
    
    // Referencia al ticket que reservó este número (microservicio Order)
    public Guid? TicketId { get; set; }

    // Navegación interna del microservicio Lottery
    public virtual LotteryDraw Lottery { get; set; } = null!;
}