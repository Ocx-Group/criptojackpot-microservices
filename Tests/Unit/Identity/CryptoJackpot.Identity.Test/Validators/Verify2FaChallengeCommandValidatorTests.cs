using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Validators;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Validators;

public class Verify2FaChallengeCommandValidatorTests
{
    private readonly Verify2FaChallengeCommandValidator _sut = new();

    private static Verify2FaChallengeCommand Valid() => new()
    {
        ChallengeToken = "valid_challenge_token",
        Code = "123456"
    };

    [Fact]
    public void Validate_ValidWithTotpCode_NoErrors()
        => _sut.Validate(Valid()).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_ValidWithRecoveryCode_NoErrors()
    {
        var cmd = new Verify2FaChallengeCommand
        {
            ChallengeToken = "token",
            RecoveryCode = "ABCD-EFGH"
        };
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyChallengeToken_ReturnsError(string? token)
    {
        var cmd = Valid(); cmd.ChallengeToken = token!;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "ChallengeToken");
    }

    [Fact]
    public void Validate_NeitherCodeNorRecoveryCode_ReturnsError()
    {
        var cmd = new Verify2FaChallengeCommand
        {
            ChallengeToken = "token",
            Code = null,
            RecoveryCode = null
        };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("12345")]   // too short
    [InlineData("1234567")] // too long
    [InlineData("abcdef")]  // letters not digits
    public void Validate_InvalidTotpCodeFormat_ReturnsError(string code)
    {
        var cmd = Valid(); cmd.Code = code;
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid")]        // wrong format
    [InlineData("AB-CDEFGH")]      // wrong part lengths
    public void Validate_InvalidRecoveryCodeFormat_ReturnsError(string recoveryCode)
    {
        var cmd = new Verify2FaChallengeCommand
        {
            ChallengeToken = "token",
            RecoveryCode = recoveryCode
        };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("ABCDEFGH")]    // no dash - still valid per regex
    [InlineData("ABCD-EFGH")]   // with dash
    public void Validate_ValidRecoveryCodeFormats_IsValid(string recoveryCode)
    {
        var cmd = new Verify2FaChallengeCommand
        {
            ChallengeToken = "token",
            RecoveryCode = recoveryCode
        };
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }
}

