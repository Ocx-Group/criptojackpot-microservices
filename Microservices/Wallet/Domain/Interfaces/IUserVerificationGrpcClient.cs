namespace CryptoJackpot.Wallet.Domain.Interfaces;

public interface IUserVerificationGrpcClient
{
    Task<UserInfoResult?> GetUserInfoAsync(Guid userGuid, CancellationToken ct = default);
    Task<bool> VerifyTotpCodeAsync(Guid userGuid, string code, CancellationToken ct = default);
}

public class UserInfoResult
{
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool TwoFactorEnabled { get; set; }
}
