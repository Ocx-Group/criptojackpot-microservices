using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Validators;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Validators;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _sut = new();

    private static RefreshTokenCommand Valid() => new()
    {
        RefreshToken = "this-is-a-valid-refresh-token-abc123"
    };

    [Fact]
    public void Validate_ValidCommand_NoErrors()
        => _sut.Validate(Valid()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyToken_ReturnsError(string? token)
    {
        var cmd = Valid(); cmd.RefreshToken = token!;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "RefreshToken");
    }

    [Fact]
    public void Validate_TokenTooShort_ReturnsError()
    {
        var cmd = Valid(); cmd.RefreshToken = "short";
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_TokenExactlyMinLength_IsValid()
    {
        var cmd = Valid(); cmd.RefreshToken = new string('x', 20);
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }
}

