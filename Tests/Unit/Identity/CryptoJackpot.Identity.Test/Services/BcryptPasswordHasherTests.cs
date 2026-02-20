using CryptoJackpot.Identity.Application.Services;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Services;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _sut = new();

    [Fact]
    public void Hash_ReturnsNonEmptyHash()
    {
        var hash = _sut.Hash("MyPassword1!");
        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Hash_SamePasswordProducesDifferentHashes()
    {
        // BCrypt uses random salt per call
        var hash1 = _sut.Hash("MyPassword1!");
        var hash2 = _sut.Hash("MyPassword1!");
        hash1.Should().NotBe(hash2, "BCrypt uses random salt, so hashes should differ");
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        const string password = "SecurePass123!";
        var hash = _sut.Hash(password);
        _sut.Verify(hash, password).Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("CorrectPassword1!");
        _sut.Verify(hash, "WrongPassword1!").Should().BeFalse();
    }

    [Fact]
    public void Verify_InvalidHash_ReturnsFalse()
    {
        // Should not throw, just return false
        _sut.Verify("not_a_valid_bcrypt_hash", "password").Should().BeFalse();
    }

    [Fact]
    public void Verify_EmptyHash_ReturnsFalse()
    {
        _sut.Verify("", "password").Should().BeFalse();
    }

    [Theory]
    [InlineData("Short1!")]
    [InlineData("AVeryLongPasswordWithLotsOfChars123!@#")]
    public void Verify_VariousPasswordLengths_WorksCorrectly(string password)
    {
        var hash = _sut.Hash(password);
        _sut.Verify(hash, password).Should().BeTrue();
    }
}

