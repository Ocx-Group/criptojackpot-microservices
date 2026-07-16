namespace CryptoJackpot.Notification.Application.Constants;

public static class TemplateNames
{
    public const string ConfirmEmail = "ConfirmEmailTemplate";
    public const string WelcomeEmail = "WelcomeEmailTemplate";
    public const string PasswordReset = "PasswordResetTemplate";
    public const string ReferralCommissionCredited = "ReferralCommissionCreditedTemplate";
    public const string LotteryMarketing = "LotteryMarketingTemplate";
    public const string PurchaseConfirmation = "PurchaseConfirmationTemplate";
    public const string WithdrawalVerification = "WithdrawalVerificationTemplate";
    public const string WinnerNotification = "WinnerNotificationTemplate";
}

public static class UrlPaths
{
    public const string ConfirmEmail = "/user_confirm_email";
    public const string ReferralProgram = "/referal-program";
    public const string LotteryDetails = "/lottery";
    public const string MyTickets = "/my-tickets";
    public const string MyWallet = "/my-wallet";
    public const string Transactions = "/transaction";
}
