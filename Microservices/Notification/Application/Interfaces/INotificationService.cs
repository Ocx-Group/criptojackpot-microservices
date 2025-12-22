using CryptoJackpot.Domain.Core.Responses;

namespace CryptoJackpot.Notification.Application.Interfaces;

public interface INotificationService
{
    Task<ResultResponse<bool>> SendEmailConfirmationAsync(long userId, string email, string name, string lastName, string token);
    Task<ResultResponse<bool>> SendPasswordResetEmailAsync(string email, string name, string lastName, string securityCode);
    Task<ResultResponse<bool>> SendReferralNotificationAsync(string referrerEmail, string referrerName, string referrerLastName, string referredName, string referredLastName, string referralCode);
}
