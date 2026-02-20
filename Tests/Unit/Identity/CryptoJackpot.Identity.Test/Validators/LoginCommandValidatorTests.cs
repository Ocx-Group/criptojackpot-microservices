using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Validators;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _sut = new();

    private static LoginCommand Valid() => new()
    {
        Email = "user@cryptojackpot.com",
        Password = "SecurePass1!"
    };

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var result = _sut.Validate(Valid());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-an-email")]
    public void Validate_InvalidEmail_ReturnsError(string? email)
    {
        var cmd = Valid(); cmd.Email = email!;
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_EmailTooLong_ReturnsError()
    {
        var cmd = Valid(); cmd.Email = new string('a', 145) + "@x.com"; // 151 chars > 150 limit
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("short")]
    public void Validate_InvalidPassword_ReturnsError(string? password)
    {
        var cmd = Valid(); cmd.Password = password!;
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_PasswordTooLong_ReturnsError()
    {
        var cmd = Valid(); cmd.Password = new string('a', 129);
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }
}

