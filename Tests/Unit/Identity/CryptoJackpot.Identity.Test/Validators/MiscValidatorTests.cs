using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Validators;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Validators;

public class RequestPasswordResetCommandValidatorTests
{
    private readonly RequestPasswordResetCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidEmail_NoErrors()
    {
        var cmd = new RequestPasswordResetCommand { Email = "user@example.com" };
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public void Validate_InvalidEmail_ReturnsError(string? email)
    {
        var cmd = new RequestPasswordResetCommand { Email = email! };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EmailTooLong_ReturnsError()
    {
        var cmd = new RequestPasswordResetCommand { Email = new string('a', 145) + "@x.com" }; // 151 chars > 150 limit
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }
}

public class Confirm2FaCommandValidatorTests
{
    private readonly Confirm2FaCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var cmd = new Confirm2FaCommand { UserGuid = Guid.NewGuid(), Code = "123456" };
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyUserGuid_ReturnsError()
    {
        var cmd = new Confirm2FaCommand { UserGuid = Guid.Empty, Code = "123456" };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("12345")]
    [InlineData("abcdef")]
    public void Validate_InvalidCode_ReturnsError(string? code)
    {
        var cmd = new Confirm2FaCommand { UserGuid = Guid.NewGuid(), Code = code! };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }
}

public class Regenerate2FaRecoveryCodesCommandValidatorTests
{
    private readonly Regenerate2FaRecoveryCodesCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var cmd = new Regenerate2FaRecoveryCodesCommand { UserGuid = Guid.NewGuid(), Code = "123456" };
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyUserGuid_ReturnsError()
    {
        var cmd = new Regenerate2FaRecoveryCodesCommand { UserGuid = Guid.Empty, Code = "123456" };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }
}

public class Setup2FaCommandValidatorTests
{
    private readonly Setup2FaCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var cmd = new Setup2FaCommand { UserGuid = Guid.NewGuid() };
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyUserGuid_ReturnsError()
    {
        var cmd = new Setup2FaCommand { UserGuid = Guid.Empty };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }
}

public class LogoutAllDevicesCommandValidatorTests
{
    private readonly LogoutAllDevicesCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var cmd = new LogoutAllDevicesCommand { UserGuid = Guid.NewGuid() };
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyUserGuid_ReturnsError()
    {
        var cmd = new LogoutAllDevicesCommand { UserGuid = Guid.Empty };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }
}

public class GoogleLoginCommandValidatorTests
{
    private readonly GoogleLoginCommandValidator _sut = new();

    [Fact]
    public void Validate_ValidIdToken_NoErrors()
    {
        var cmd = new GoogleLoginCommand { IdToken = new string('x', 100) };
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyIdToken_ReturnsError(string? token)
    {
        var cmd = new GoogleLoginCommand { IdToken = token! };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_IdTokenTooShort_ReturnsError()
    {
        var cmd = new GoogleLoginCommand { IdToken = "short_token" };
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }
}
