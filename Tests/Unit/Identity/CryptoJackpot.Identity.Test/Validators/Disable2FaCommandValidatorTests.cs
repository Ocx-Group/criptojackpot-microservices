using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Validators;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Validators;

public class Disable2FaCommandValidatorTests
{
    private readonly Disable2FaCommandValidator _sut = new();

    private static Disable2FaCommand Valid() => new()
    {
        UserGuid = Guid.NewGuid(),
        Code = "123456"
    };

    [Fact]
    public void Validate_ValidWithTotpCode_NoErrors()
        => _sut.Validate(Valid()).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_ValidWithRecoveryCode_NoErrors()
    {
        var cmd = new Disable2FaCommand { UserGuid = Guid.NewGuid(), RecoveryCode = "ABCD-EFGH" };
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyUserGuid_ReturnsError()
    {
        var cmd = Valid(); cmd.UserGuid = Guid.Empty;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "UserGuid");
    }

    [Fact]
    public void Validate_NeitherCodeNorRecoveryCode_ReturnsError()
    {
        var cmd = new Disable2FaCommand { UserGuid = Guid.NewGuid() };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abcdef")]
    public void Validate_InvalidTotpCodeFormat_ReturnsError(string code)
    {
        var cmd = Valid(); cmd.Code = code;
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_InvalidRecoveryCodeFormat_ReturnsError()
    {
        var cmd = new Disable2FaCommand { UserGuid = Guid.NewGuid(), RecoveryCode = "invalid" };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }
}

