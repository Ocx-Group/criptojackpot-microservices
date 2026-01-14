namespace CryptoJackpot.Lottery.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando se intenta reservar un número que ya fue reservado por otro usuario (concurrencia)
/// </summary>
public class DuplicateNumberReservationException : Exception
{
    public Guid LotteryId { get; }
    public Guid TicketId { get; }

    public DuplicateNumberReservationException(Guid lotteryId, Guid ticketId)
        : base("One or more numbers were reserved by another user.")
    {
        LotteryId = lotteryId;
        TicketId = ticketId;
    }

    public DuplicateNumberReservationException(Guid lotteryId, Guid ticketId, Exception innerException)
        : base("One or more numbers were reserved by another user.", innerException)
    {
        LotteryId = lotteryId;
        TicketId = ticketId;
    }
}

