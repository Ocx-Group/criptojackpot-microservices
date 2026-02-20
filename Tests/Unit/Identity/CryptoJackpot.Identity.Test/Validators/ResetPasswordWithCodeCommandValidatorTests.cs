using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Validators;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Validators;

public class ResetPasswordWithCodeCommandValidatorTests
{
    private readonly ResetPasswordWithCodeCommandValidator _sut = new();

    private static ResetPasswordWithCodeCommand Valid() => new()
    {
        Email = "user@example.com",
        SecurityCode = "123456",
        Password = "NewPass1!",
        ConfirmPassword = "NewPass1!"
    };

    [Fact]
    public void Validate_ValidCommand_NoErrors()
        => _sut.Validate(Valid()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("")]
    [InlineData("not-email")]
    public void Validate_InvalidEmail_ReturnsError(string email)
    {
        var cmd = Valid(); cmd.Email = email;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("12345")]   // 5 digits
    [InlineData("1234567")] // 7 digits
    [InlineData("abcdef")]  // letters
    [InlineData("")]
    public void Validate_InvalidSecurityCode_ReturnsError(string code)
    {
        var cmd = Valid(); cmd.SecurityCode = code;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "SecurityCode");
    }

    [Fact]
    public void Validate_SecurityCodeExactly6Digits_IsValid()
    {
        var cmd = Valid(); cmd.SecurityCode = "000000";
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_PasswordTooShort_ReturnsError()
    {
        var cmd = Valid(); cmd.Password = "Ab1!";
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_ConfirmPasswordMismatch_ReturnsError()
    {
        var cmd = Valid(); cmd.ConfirmPassword = "DifferentPass1!";
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyConfirmPassword_ReturnsError(string? confirm)
    {
        var cmd = Valid(); cmd.ConfirmPassword = confirm!;
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }
}

