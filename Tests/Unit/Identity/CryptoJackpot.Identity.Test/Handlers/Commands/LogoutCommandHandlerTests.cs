using CryptoJackpot.Identity.Application.Commands;
using CryptoJackpot.Identity.Application.Handlers.Commands;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Commands;

public class LogoutCommandHandlerTests
{
    // ─── Mocks ──────────────────────────────────────────────────────
    private readonly IAuthenticationService _authService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly LogoutCommandHandler _sut;

    public LogoutCommandHandlerTests()
    {
        _authService = Substitute.For<IAuthenticationService>();
        _refreshTokenService = Substitute.For<IRefreshTokenService>();
        _eventPublisher = Substitute.For<IIdentityEventPublisher>();
        _sut = new LogoutCommandHandler(_authService, _refreshTokenService, _eventPublisher);
    }

    // ─── Helpers ────────────────────────────────────────────────────

    private static UserRefreshToken CreateActiveToken(User user) => new()
    {
        TokenHash = "some_hash",
        User = user
    };

    private static User CreateUser() => new()
    {
        Id = 1,
        UserGuid = Guid.NewGuid(),
        Email = "user@cryptojackpot.com",
        Name = "John",
        LastName = "Doe"
    };

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: Valid refresh token → revoked, user resolved, audit published
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithValidRefreshToken_RevokesTokenAndReturnsOk()
    {
        // Arrange
        const string refreshToken = "valid_refresh_token_abc";
        var user = CreateUser();
        var tokenEntity = CreateActiveToken(user);
        _refreshTokenService.ValidateAndGetTokenAsync(refreshToken).Returns(tokenEntity);

        var command = new LogoutCommand { RefreshToken = refreshToken };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _authService.Received(1).RevokeRefreshTokenAsync(refreshToken, Arg.Any<CancellationToken>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Null refresh token → skips revocation and audit
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithNullRefreshToken_DoesNotRevokeAndReturnsOk()
    {
        // Arrange
        var command = new LogoutCommand { RefreshToken = null };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _authService.DidNotReceive().RevokeRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _eventPublisher.DidNotReceive().PublishUserLoggedOutAsync(Arg.Any<User>(), Arg.Any<string?>(), Arg.Any<string?>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Empty refresh token → skips revocation and audit
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithEmptyRefreshToken_DoesNotRevokeAndReturnsOk()
    {
        // Arrange
        var command = new LogoutCommand { RefreshToken = string.Empty };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _authService.DidNotReceive().RevokeRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _eventPublisher.DidNotReceive().PublishUserLoggedOutAsync(Arg.Any<User>(), Arg.Any<string?>(), Arg.Any<string?>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: Whitespace refresh token → skips revocation and audit
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithWhitespaceRefreshToken_DoesNotRevokeAndReturnsOk()
    {
        // Arrange
        var command = new LogoutCommand { RefreshToken = "   " };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _authService.DidNotReceive().RevokeRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _eventPublisher.DidNotReceive().PublishUserLoggedOutAsync(Arg.Any<User>(), Arg.Any<string?>(), Arg.Any<string?>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 5: CancellationToken propagated to revocation call
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        const string token = "some_refresh_token";
        _refreshTokenService.ValidateAndGetTokenAsync(token).Returns((UserRefreshToken?)null);
        var command = new LogoutCommand { RefreshToken = token };

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert
        await _authService.Received(1).RevokeRefreshTokenAsync(token, cts.Token);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 6: Token not found / expired → revokes, skips audit
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WhenTokenNotFound_RevokesButDoesNotPublishAudit()
    {
        // Arrange
        const string refreshToken = "expired_or_invalid_token";
        _refreshTokenService.ValidateAndGetTokenAsync(refreshToken).Returns((UserRefreshToken?)null);
        var command = new LogoutCommand { RefreshToken = refreshToken };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _authService.Received(1).RevokeRefreshTokenAsync(refreshToken, Arg.Any<CancellationToken>());
        await _eventPublisher.DidNotReceive().PublishUserLoggedOutAsync(Arg.Any<User>(), Arg.Any<string?>(), Arg.Any<string?>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 7: Valid token + user → audit event published with IP and UserAgent
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_WithValidTokenAndUser_PublishesAuditEvent()
    {
        // Arrange
        const string refreshToken = "valid_token";
        const string ipAddress = "192.168.1.1";
        const string userAgent = "Mozilla/5.0";
        var user = CreateUser();
        var tokenEntity = CreateActiveToken(user);
        _refreshTokenService.ValidateAndGetTokenAsync(refreshToken).Returns(tokenEntity);

        var command = new LogoutCommand
        {
            RefreshToken = refreshToken,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _eventPublisher.Received(1).PublishUserLoggedOutAsync(user, ipAddress, userAgent);
    }
}
