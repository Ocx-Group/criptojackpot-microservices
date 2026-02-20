using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Validators;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Validators;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _sut = new();

    private static CreateUserCommand Valid() => new()
    {
        Name = "John",
        LastName = "Doe",
        Email = "john@example.com",
        Password = "SecurePass1!",
        CountryId = 1,
        StatePlace = "California",
        City = "Los Angeles"
    };

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var result = _sut.Validate(Valid());
        result.IsValid.Should().BeTrue();
    }

    // ── Name ─────────────────────────────────────────────────────────
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyName_ReturnsError(string? name)
    {
        var cmd = Valid(); cmd.Name = name!;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_NameTooLong_ReturnsError()
    {
        var cmd = Valid(); cmd.Name = new string('a', 101);
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    // ── LastName ─────────────────────────────────────────────────────
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyLastName_ReturnsError(string? lastName)
    {
        var cmd = Valid(); cmd.LastName = lastName!;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    // ── Email ────────────────────────────────────────────────────────
    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public void Validate_InvalidEmail_ReturnsError(string email)
    {
        var cmd = Valid(); cmd.Email = email;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    // ── Password ─────────────────────────────────────────────────────
    [Fact]
    public void Validate_PasswordTooShort_ReturnsError()
    {
        var cmd = Valid(); cmd.Password = "Ab1!";
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_PasswordNoUppercase_ReturnsError()
    {
        var cmd = Valid(); cmd.Password = "lowercase1!";
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_PasswordNoLowercase_ReturnsError()
    {
        var cmd = Valid(); cmd.Password = "UPPERCASE1!";
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_PasswordNoDigit_ReturnsError()
    {
        var cmd = Valid(); cmd.Password = "NoDigitPass!";
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_PasswordNoSpecialChar_ReturnsError()
    {
        var cmd = Valid(); cmd.Password = "NoSpecial1Pass";
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // ── CountryId ────────────────────────────────────────────────────
    [Fact]
    public void Validate_CountryIdZero_ReturnsError()
    {
        var cmd = Valid(); cmd.CountryId = 0;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "CountryId");
    }

    // ── StatePlace / City ─────────────────────────────────────────────
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyStatePlace_ReturnsError(string? statePlace)
    {
        var cmd = Valid(); cmd.StatePlace = statePlace!;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "StatePlace");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyCity_ReturnsError(string? city)
    {
        var cmd = Valid(); cmd.City = city!;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "City");
    }

    // ── Optional fields ───────────────────────────────────────────────
    [Fact]
    public void Validate_NullOptionalFields_IsValid()
    {
        var cmd = Valid();
        cmd.Phone = null;
        cmd.Identification = null;
        cmd.Address = null;
        cmd.ReferralCode = null;
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_PhoneTooLong_ReturnsError()
    {
        var cmd = Valid(); cmd.Phone = new string('1', 21);
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ReferralCodeTooLong_ReturnsError()
    {
        var cmd = Valid(); cmd.ReferralCode = new string('X', 51);
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }
}

