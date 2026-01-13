using FluentResults;
using MediatR;

namespace CryptoJackpot.Lottery.Application.Commands;

public class ReleaseNumbersCommand : IRequest<Result<bool>>
{
    public Guid TicketId { get; set; }
}

