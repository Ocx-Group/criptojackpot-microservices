using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Domain;

public class UserDomainMethodsTests
{
    private static User CreateUser(int failedAttempts = 0, DateTime? lockoutEnd = null) => new()
    {
        Id = 1,
        Email = "user@test.com",
        Name = "John",
        LastName = "Doe",
        FailedLoginAttempts = failedAttempts,
        LockoutEndAt = lockoutEnd
    };

    // ═════════════════════════════════════════════════════════════════
    // RegisterFailedLogin – lockout thresholds
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void RegisterFailedLogin_IncreasesFailedAttempts()
    {
        var user = CreateUser(failedAttempts: 0);
        user.RegisterFailedLogin();
        user.FailedLoginAttempts.Should().Be(1);
    }

    [Theory]
    [InlineData(0, false)]  // 1 attempt → no lockout
    [InlineData(1, false)]  // 2 attempts → no lockout
    [InlineData(2, true)]   // 3 attempts → lockout 1 min
    [InlineData(4, true)]   // 5 attempts → lockout 5 min
    [InlineData(6, true)]   // 7 attempts → lockout 30 min
    public void RegisterFailedLogin_LockoutThresholds_SetsLockoutEndAt(int initialAttempts, bool expectsLockout)
    {
        var user = CreateUser(failedAttempts: initialAttempts);
        user.RegisterFailedLogin();
        if (expectsLockout)
            user.LockoutEndAt.Should().NotBeNull().And.BeAfter(DateTime.UtcNow);
        else
            user.LockoutEndAt.Should().BeNull();
    }

    [Fact]
    public void RegisterFailedLogin_At3Attempts_Locks1Minute()
    {
        var user = CreateUser(failedAttempts: 2); // will become 3
        var before = DateTime.UtcNow;
        user.RegisterFailedLogin();
        user.LockoutEndAt.Should().BeCloseTo(before.AddMinutes(1), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void RegisterFailedLogin_At5Attempts_Locks5Minutes()
    {
        var user = CreateUser(failedAttempts: 4); // will become 5
        var before = DateTime.UtcNow;
        user.RegisterFailedLogin();
        user.LockoutEndAt.Should().BeCloseTo(before.AddMinutes(5), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void RegisterFailedLogin_At7Attempts_Locks30Minutes()
    {
        var user = CreateUser(failedAttempts: 6); // will become 7
        var before = DateTime.UtcNow;
        user.RegisterFailedLogin();
        user.LockoutEndAt.Should().BeCloseTo(before.AddMinutes(30), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void RegisterFailedLogin_UpdatesUpdatedAt()
    {
        var user = CreateUser();
        var before = DateTime.UtcNow;
        user.RegisterFailedLogin();
        user.UpdatedAt.Should().BeOnOrAfter(before);
    }

    // ═════════════════════════════════════════════════════════════════
    // RegisterSuccessfulLogin
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void RegisterSuccessfulLogin_ResetsFailedAttempts()
    {
        var user = CreateUser(failedAttempts: 5);
        user.RegisterSuccessfulLogin();
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void RegisterSuccessfulLogin_ClearsLockout()
    {
        var user = CreateUser(lockoutEnd: DateTime.UtcNow.AddMinutes(30));
        user.RegisterSuccessfulLogin();
        user.LockoutEndAt.Should().BeNull();
    }

    [Fact]
    public void RegisterSuccessfulLogin_SetsLastLoginAt()
    {
        var user = CreateUser();
        var before = DateTime.UtcNow;
        user.RegisterSuccessfulLogin();
        user.LastLoginAt.Should().NotBeNull().And.BeOnOrAfter(before);
    }

    [Fact]
    public void RegisterSuccessfulLogin_UpdatesUpdatedAt()
    {
        var user = CreateUser();
        var before = DateTime.UtcNow;
        user.RegisterSuccessfulLogin();
        user.UpdatedAt.Should().BeOnOrAfter(before);
    }

    // ═════════════════════════════════════════════════════════════════
    // IsLockedOut property
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void IsLockedOut_FutureLockoutEnd_ReturnsTrue()
    {
        var user = CreateUser(lockoutEnd: DateTime.UtcNow.AddMinutes(5));
        user.IsLockedOut.Should().BeTrue();
    }

    [Fact]
    public void IsLockedOut_PastLockoutEnd_ReturnsFalse()
    {
        var user = CreateUser(lockoutEnd: DateTime.UtcNow.AddMinutes(-1));
        user.IsLockedOut.Should().BeFalse();
    }

    [Fact]
    public void IsLockedOut_NullLockoutEnd_ReturnsFalse()
    {
        var user = CreateUser(lockoutEnd: null);
        user.IsLockedOut.Should().BeFalse();
    }

    // ═════════════════════════════════════════════════════════════════
    // IsExternalOnly property
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void IsExternalOnly_NullPasswordAndGoogleId_ReturnsTrue()
    {
        var user = new User { PasswordHash = null, GoogleId = "google123" };
        user.IsExternalOnly.Should().BeTrue();
    }

    [Fact]
    public void IsExternalOnly_HasPassword_ReturnsFalse()
    {
        var user = new User { PasswordHash = "hash", GoogleId = "google123" };
        user.IsExternalOnly.Should().BeFalse();
    }

    [Fact]
    public void IsExternalOnly_NullGoogleId_ReturnsFalse()
    {
        var user = new User { PasswordHash = null, GoogleId = null };
        user.IsExternalOnly.Should().BeFalse();
    }
}

