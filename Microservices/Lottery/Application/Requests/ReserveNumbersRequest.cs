namespace CryptoJackpot.Lottery.Application.Requests;

public class ReserveNumbersRequest
{
    public Guid TicketId { get; set; }
    public List<int> Numbers { get; set; } = [];
    public int Series { get; set; }
}

