using CryptoJackpot.Wallet.Application.DTOs;
using FluentResults;
using MediatR;

namespace CryptoJackpot.Wallet.Application.Commands;

public class ProcessWithdrawalRequestCommand : IRequest<Result<WithdrawalRequestDto>>
{
    public Guid RequestGuid { get; set; }
    public bool Approve { get; set; }
    public string? AdminNotes { get; set; }
}
