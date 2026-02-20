using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Validators;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Validators;

public class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _sut = new();

    private static UpdateUserCommand Valid() => new()
    {
        UserId = 1,
        Name = "John",
        LastName = "Doe",
        CountryId = 1,
        StatePlace = "California",
        City = "Los Angeles"
    };

    [Fact]
    public void Validate_ValidCommand_NoErrors()
        => _sut.Validate(Valid()).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_UserIdZero_ReturnsError()
    {
        var cmd = Valid(); cmd.UserId = 0;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

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

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyLastName_ReturnsError(string? lastName)
    {
        var cmd = Valid(); cmd.LastName = lastName!;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    [Fact]
    public void Validate_CountryIdZero_ReturnsError()
    {
        var cmd = Valid(); cmd.CountryId = 0;
        _sut.Validate(cmd).Errors.Should().Contain(e => e.PropertyName == "CountryId");
    }

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

    [Fact]
    public void Validate_PhoneTooLong_ReturnsError()
    {
        var cmd = Valid(); cmd.Phone = new string('1', 21);
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_NullPhoneIsValid()
    {
        var cmd = Valid(); cmd.Phone = null;
        _sut.Validate(cmd).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_AddressTooLong_ReturnsError()
    {
        var cmd = Valid(); cmd.Address = new string('a', 151);
        _sut.Validate(cmd).IsValid.Should().BeFalse();
    }
}

