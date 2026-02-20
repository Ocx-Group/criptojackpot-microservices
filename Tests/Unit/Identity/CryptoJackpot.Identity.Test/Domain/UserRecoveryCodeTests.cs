using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Domain;

public class UserRecoveryCodeTests
{
    // ═════════════════════════════════════════════════════════════════
    // MarkAsUsed
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void MarkAsUsed_SetsIsUsedTrue()
    {
        var code = new UserRecoveryCode { IsUsed = false };
        code.MarkAsUsed();
        code.IsUsed.Should().BeTrue();
    }

    [Fact]
    public void MarkAsUsed_SetsUsedAt()
    {
        var before = DateTime.UtcNow;
        var code = new UserRecoveryCode { IsUsed = false };
        code.MarkAsUsed();
        code.UsedAt.Should().NotBeNull().And.BeOnOrAfter(before);
    }

    [Fact]
    public void MarkAsUsed_CalledTwice_StillUsed()
    {
        var code = new UserRecoveryCode { IsUsed = false };
        code.MarkAsUsed();
        code.MarkAsUsed();
        code.IsUsed.Should().BeTrue();
    }
}

public class UserRefreshTokenTests
{
    [Fact]
    public void UserRefreshToken_DefaultsAreCorrect()
    {
        var token = new UserRefreshToken();
        token.IsRevoked.Should().BeFalse();
        token.RevokedAt.Should().BeNull();
        token.RevokedReason.Should().BeNull();
    }

    [Fact]
    public void Revoke_SetsIsRevokedTrue()
    {
        var token = new UserRefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(7) };
        token.Revoke("logout");
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Revoke_SetsRevokedAtAndReason()
    {
        var before = DateTime.UtcNow;
        var token = new UserRefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(7) };
        token.Revoke("security_alert");
        token.RevokedAt.Should().NotBeNull().And.BeOnOrAfter(before);
        token.RevokedReason.Should().Be("security_alert");
    }

    [Fact]
    public void Revoke_WithReplacedByHash_SetsReplacedByTokenHash()
    {
        var token = new UserRefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(7) };
        token.Revoke("rotation", "new_hash_abc");
        token.ReplacedByTokenHash.Should().Be("new_hash_abc");
    }

    [Fact]
    public void IsActive_NotRevokedAndNotExpired_ReturnsTrue()
    {
        var token = new UserRefreshToken
        {
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_Revoked_ReturnsFalse()
    {
        var token = new UserRefreshToken
        {
            IsRevoked = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_Expired_ReturnsFalse()
    {
        var token = new UserRefreshToken
        {
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddSeconds(-1)
        };
        token.IsActive.Should().BeFalse();
    }
}

