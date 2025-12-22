using CryptoJackpot.Domain.Core.Responses;
using CryptoJackpot.Notification.Application.Commands;
using CryptoJackpot.Notification.Application.Interfaces;
using MediatR;

namespace CryptoJackpot.Notification.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IMediator _mediator;

    public NotificationService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<ResultResponse<bool>> SendEmailConfirmationAsync(
        long userId, string email, string name, string lastName, string token)
    {
        return await _mediator.Send(new SendEmailConfirmationCommand
        {
            UserId = userId,
            Email = email,
            Name = name,
            LastName = lastName,
            Token = token
        });
    }

    public async Task<ResultResponse<bool>> SendPasswordResetEmailAsync(
        string email, string name, string lastName, string securityCode)
    {
        return await _mediator.Send(new SendPasswordResetCommand
        {
            Email = email,
            Name = name,
            LastName = lastName,
            SecurityCode = securityCode
        });
    }

    public async Task<ResultResponse<bool>> SendReferralNotificationAsync(
        string referrerEmail, string referrerName, string referrerLastName,
        string referredName, string referredLastName, string referralCode)
    {
        return await _mediator.Send(new SendReferralNotificationCommand
        {
            ReferrerEmail = referrerEmail,
            ReferrerName = referrerName,
            ReferrerLastName = referrerLastName,
            ReferredName = referredName,
            ReferredLastName = referredLastName,
            ReferralCode = referralCode
        });
    }
}
