namespace CryptoJackpot.Wallet.Application.Requests;

public class AdminCreditRequest
{
    public Guid UserGuid { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
