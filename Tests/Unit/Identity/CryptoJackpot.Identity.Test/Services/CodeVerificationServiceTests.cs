using CryptoJackpot.Domain.Core.Responses.Errors;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Application.Services;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Services;

public class CodeVerificationServiceTests
{
    private readonly ITotpService _totpService;
    private readonly IRecoveryCodeService _recoveryCodeService;
    private readonly IDataEncryptionService _encryptionService;
    private readonly CodeVerificationService _sut;

    public CodeVerificationServiceTests()
    {
        _totpService = Substitute.For<ITotpService>();
        _recoveryCodeService = Substitute.For<IRecoveryCodeService>();
        _encryptionService = Substitute.For<IDataEncryptionService>();
        var logger = Substitute.For<ILogger<CodeVerificationService>>();

        _sut = new CodeVerificationService(
            _totpService,
            _recoveryCodeService,
            _encryptionService,
            logger);
    }

    private static User CreateUserWith2Fa(
        string? secret = "ENCRYPTED_SECRET",
        bool hasCodes = false)
    {
        var user = new User
        {
            Id = 1,
            UserGuid = Guid.NewGuid(),
            Email = "user@test.com",
            Name = "John",
            LastName = "Doe",
            TwoFactorEnabled = true,
            TwoFactorSecret = secret
        };
        if (hasCodes)
        {
            user.RecoveryCodes.Add(new UserRecoveryCode
            {
                Id = 1, UserId = 1, CodeHash = "hash_abc", IsUsed = false
            });
        }
        return user;
    }

    // ═════════════════════════════════════════════════════════════════
    // No code provided
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void VerifyCode_NoBothCodes_ReturnsUnauthorizedError()
    {
        var user = CreateUserWith2Fa();
        var result = _sut.VerifyCode(user, null, null);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>();
    }

    // ═════════════════════════════════════════════════════════════════
    // TOTP path
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void VerifyCode_TotpCode_NoSecret_ReturnsUnauthorizedError()
    {
        var user = CreateUserWith2Fa(secret: null);
        var result = _sut.VerifyCode(user, totpCode: "123456", recoveryCode: null);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>();
    }

    [Fact]
    public void VerifyCode_TotpCode_DecryptionReturnsEmpty_ReturnsUnauthorizedError()
    {
        var user = CreateUserWith2Fa(secret: "ENCRYPTED");
        _encryptionService.Decrypt("ENCRYPTED").Returns(string.Empty);

        var result = _sut.VerifyCode(user, totpCode: "123456", recoveryCode: null);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>();
    }

    [Fact]
    public void VerifyCode_TotpCode_ValidCode_ReturnsOk()
    {
        var user = CreateUserWith2Fa();
        _encryptionService.Decrypt("ENCRYPTED_SECRET").Returns("DECRYPTED_SECRET");
        _totpService.ValidateCode("DECRYPTED_SECRET", "123456").Returns(true);

        var result = _sut.VerifyCode(user, totpCode: "123456", recoveryCode: null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull("no used recovery code on TOTP path");
    }

    [Fact]
    public void VerifyCode_TotpCode_InvalidCode_ReturnsUnauthorizedError()
    {
        var user = CreateUserWith2Fa();
        _encryptionService.Decrypt("ENCRYPTED_SECRET").Returns("DECRYPTED_SECRET");
        _totpService.ValidateCode("DECRYPTED_SECRET", "000000").Returns(false);

        var result = _sut.VerifyCode(user, totpCode: "000000", recoveryCode: null);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>();
    }

    // ═════════════════════════════════════════════════════════════════
    // Recovery code path
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void VerifyCode_RecoveryCode_ValidCode_ReturnsOkWithUsedCode()
    {
        var user = CreateUserWith2Fa(hasCodes: true);
        var matchingCode = user.RecoveryCodes.First();
        _recoveryCodeService.ValidateCode("ABCD-EFGH", user.RecoveryCodes)
            .Returns(matchingCode);

        var result = _sut.VerifyCode(user, totpCode: null, recoveryCode: "ABCD-EFGH");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Recovery code accepted");
        matchingCode.IsUsed.Should().BeTrue("MarkAsUsed should have been called");
    }

    [Fact]
    public void VerifyCode_RecoveryCode_InvalidCode_ReturnsUnauthorizedError()
    {
        var user = CreateUserWith2Fa(hasCodes: true);
        _recoveryCodeService.ValidateCode(Arg.Any<string>(), Arg.Any<IEnumerable<UserRecoveryCode>>())
            .Returns((UserRecoveryCode?)null);

        var result = _sut.VerifyCode(user, totpCode: null, recoveryCode: "XXXX-YYYY");

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>();
    }

    // ═════════════════════════════════════════════════════════════════
    // TOTP takes priority over recovery code when both provided
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void VerifyCode_BothCodes_TotpTakesPriority()
    {
        var user = CreateUserWith2Fa();
        _encryptionService.Decrypt(Arg.Any<string>()).Returns("DECRYPTED");
        _totpService.ValidateCode(Arg.Any<string>(), "123456").Returns(true);

        _sut.VerifyCode(user, totpCode: "123456", recoveryCode: "ABCD-EFGH");

        // Recovery code service should NOT be called
        _recoveryCodeService.DidNotReceive().ValidateCode(
            Arg.Any<string>(), Arg.Any<IEnumerable<UserRecoveryCode>>());
    }
}

