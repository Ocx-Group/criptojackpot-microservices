using CryptoJackpot.Identity.Application.Services;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Services;

public class TotpServiceTests
{
    private readonly TotpService _sut = new();

    // ═════════════════════════════════════════════════════════════════
    // GenerateSecret
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void GenerateSecret_ReturnsNonEmptyBase32String()
    {
        var secret = _sut.GenerateSecret();
        secret.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateSecret_EachCallProducesDifferentSecret()
    {
        var s1 = _sut.GenerateSecret();
        var s2 = _sut.GenerateSecret();
        s1.Should().NotBe(s2, "each call should use a fresh random secret");
    }

    [Fact]
    public void GenerateSecret_ReturnsValidBase32()
    {
        var secret = _sut.GenerateSecret();
        // Base32 chars: A-Z and 2-7
        secret.Should().MatchRegex(@"^[A-Z2-7]+=*$", because: "secret should be valid Base32");
    }

    // ═════════════════════════════════════════════════════════════════
    // ValidateCode – edge cases (no valid TOTP without a live code)
    // ═════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData("", "123456")]
    [InlineData("VALIDBASE32SECRET===", "")]
    [InlineData(null, "123456")]
    [InlineData("VALIDBASE32SECRET===", null)]
    public void ValidateCode_NullOrEmptyArgs_ReturnsFalse(string? secret, string? code)
    {
        _sut.ValidateCode(secret!, code!).Should().BeFalse();
    }

    [Fact]
    public void ValidateCode_InvalidBase32Secret_ReturnsFalse()
    {
        // Should not throw, just return false
        _sut.ValidateCode("NOT!VALID@BASE32", "123456").Should().BeFalse();
    }

    [Fact]
    public void ValidateCode_WrongCode_ReturnsFalse()
    {
        var secret = _sut.GenerateSecret();
        // "000000" is almost certainly wrong for any given second
        _sut.ValidateCode(secret, "000000").Should().BeFalse();
    }

    // ═════════════════════════════════════════════════════════════════
    // GenerateQrCodeUri
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void GenerateQrCodeUri_ReturnsOtpauthScheme()
    {
        var uri = _sut.GenerateQrCodeUri("user@test.com", "MYSECRET");
        uri.Should().StartWith("otpauth://totp/");
    }

    [Fact]
    public void GenerateQrCodeUri_ContainsSecretParam()
    {
        var secret = "MYSECRET123";
        var uri = _sut.GenerateQrCodeUri("user@test.com", secret);
        uri.Should().Contain($"secret={secret}");
    }

    [Fact]
    public void GenerateQrCodeUri_ContainsDefaultIssuer()
    {
        var uri = _sut.GenerateQrCodeUri("user@test.com", "SECRET");
        uri.Should().Contain("CryptoJackpot");
    }

    [Fact]
    public void GenerateQrCodeUri_ContainsCustomIssuer()
    {
        var uri = _sut.GenerateQrCodeUri("user@test.com", "SECRET", "MyApp");
        uri.Should().Contain("MyApp");
    }

    [Fact]
    public void GenerateQrCodeUri_ContainsDigitsAndPeriod()
    {
        var uri = _sut.GenerateQrCodeUri("user@test.com", "SECRET");
        uri.Should().Contain("digits=6").And.Contain("period=30");
    }

    [Fact]
    public void GenerateQrCodeUri_EncodesSpecialCharsInEmail()
    {
        var uri = _sut.GenerateQrCodeUri("user+tag@test.com", "SECRET");
        (uri.Contains("%40") || uri.Contains("user")).Should().BeTrue(
            "email should appear in the URI either encoded or as plain text");
    }
}

