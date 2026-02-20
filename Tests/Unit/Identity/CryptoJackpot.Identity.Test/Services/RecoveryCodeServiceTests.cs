using CryptoJackpot.Identity.Application.Services;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Services;

public class RecoveryCodeServiceTests
{
    private readonly RecoveryCodeService _sut = new();

    // ═════════════════════════════════════════════════════════════════
    // GenerateCodes
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void GenerateCodes_DefaultCount_Returns8CodesAnd8Entities()
    {
        var (plainCodes, entities) = _sut.GenerateCodes(userId: 1);

        plainCodes.Should().HaveCount(8);
        entities.Should().HaveCount(8);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    public void GenerateCodes_CustomCount_ReturnsRequestedCount(int count)
    {
        var (plainCodes, entities) = _sut.GenerateCodes(userId: 1, count: count);

        plainCodes.Should().HaveCount(count);
        entities.Should().HaveCount(count);
    }

    [Fact]
    public void GenerateCodes_PlainCodesHaveXXXXDashXXXXFormat()
    {
        var (plainCodes, _) = _sut.GenerateCodes(userId: 1);

        foreach (var code in plainCodes)
        {
            code.Should().MatchRegex(@"^[A-Z2-9]{4}-[A-Z2-9]{4}$",
                because: "codes should be in XXXX-XXXX format with allowed chars");
        }
    }

    [Fact]
    public void GenerateCodes_EntitiesHaveCorrectUserId()
    {
        const long userId = 42;
        var (_, entities) = _sut.GenerateCodes(userId);

        entities.Should().AllSatisfy(e => e.UserId.Should().Be(userId));
    }

    [Fact]
    public void GenerateCodes_EntitiesAreNotUsed()
    {
        var (_, entities) = _sut.GenerateCodes(userId: 1);

        entities.Should().AllSatisfy(e => e.IsUsed.Should().BeFalse());
    }

    [Fact]
    public void GenerateCodes_AllPlainCodesAreUnique()
    {
        var (plainCodes, _) = _sut.GenerateCodes(userId: 1, count: 8);
        plainCodes.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GenerateCodes_EntityHashesMatchHashOfPlainCode()
    {
        var (plainCodes, entities) = _sut.GenerateCodes(userId: 1);

        for (var i = 0; i < plainCodes.Count; i++)
        {
            var expectedHash = _sut.HashCode(plainCodes[i]);
            entities[i].CodeHash.Should().Be(expectedHash);
        }
    }

    // ═════════════════════════════════════════════════════════════════
    // HashCode
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void HashCode_SameInputProducesSameOutput()
    {
        var hash1 = _sut.HashCode("ABCD-EFGH");
        var hash2 = _sut.HashCode("ABCD-EFGH");
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashCode_NormalizesBeforeHashing_DashVsNoDash()
    {
        // Hash with dash and without dash should produce same result
        var hashWithDash = _sut.HashCode("ABCD-EFGH");
        var hashWithoutDash = _sut.HashCode("ABCDEFGH");
        hashWithDash.Should().Be(hashWithoutDash,
            "dash should be removed before hashing");
    }

    [Fact]
    public void HashCode_CaseInsensitive()
    {
        var upper = _sut.HashCode("ABCDEFGH");
        var lower = _sut.HashCode("abcdefgh");
        upper.Should().Be(lower, "hash should be case-insensitive");
    }

    [Fact]
    public void HashCode_ReturnsHexString()
    {
        var hash = _sut.HashCode("ABCD-EFGH");
        hash.Should().MatchRegex(@"^[0-9a-f]{64}$", because: "SHA-256 produces 64 hex chars");
    }

    // ═════════════════════════════════════════════════════════════════
    // ValidateCode
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public void ValidateCode_MatchingUnusedCode_ReturnsEntity()
    {
        const string plainCode = "ABCD-EFGH";
        var hash = _sut.HashCode(plainCode);
        var stored = new List<UserRecoveryCode>
        {
            new() { Id = 1, CodeHash = hash, IsUsed = false }
        };

        var result = _sut.ValidateCode(plainCode, stored);

        result.Should().NotBeNull();
        result!.CodeHash.Should().Be(hash);
    }

    [Fact]
    public void ValidateCode_AlreadyUsedCode_ReturnsNull()
    {
        const string plainCode = "ABCD-EFGH";
        var hash = _sut.HashCode(plainCode);
        var stored = new List<UserRecoveryCode>
        {
            new() { Id = 1, CodeHash = hash, IsUsed = true }
        };

        var result = _sut.ValidateCode(plainCode, stored);
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateCode_WrongCode_ReturnsNull()
    {
        var hash = _sut.HashCode("ABCD-EFGH");
        var stored = new List<UserRecoveryCode>
        {
            new() { Id = 1, CodeHash = hash, IsUsed = false }
        };

        var result = _sut.ValidateCode("XXXX-YYYY", stored);
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void ValidateCode_EmptyOrNullCode_ReturnsNull(string? code)
    {
        var stored = new List<UserRecoveryCode>
        {
            new() { Id = 1, CodeHash = "somehash", IsUsed = false }
        };
        _sut.ValidateCode(code!, stored).Should().BeNull();
    }

    [Fact]
    public void ValidateCode_CodeWithDashMatchesCodeWithoutDash()
    {
        // Code stored as "ABCDEFGH" should match input "ABCD-EFGH"
        var hashNoDash = _sut.HashCode("ABCDEFGH");
        var stored = new List<UserRecoveryCode>
        {
            new() { Id = 1, CodeHash = hashNoDash, IsUsed = false }
        };

        var result = _sut.ValidateCode("ABCD-EFGH", stored);
        result.Should().NotBeNull();
    }
}

