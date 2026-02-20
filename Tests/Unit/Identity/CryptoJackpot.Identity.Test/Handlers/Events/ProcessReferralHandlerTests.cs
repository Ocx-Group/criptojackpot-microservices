using CryptoJackpot.Identity.Application.Events;
using CryptoJackpot.Identity.Application.Handlers.Events;
using CryptoJackpot.Identity.Application.Interfaces;
using CryptoJackpot.Identity.Domain.Interfaces;
using CryptoJackpot.Identity.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace CryptoJackpot.Identity.Test.Handlers.Events;

public class ProcessReferralHandlerTests
{
    private readonly IUserReferralRepository _repository;
    private readonly IIdentityEventPublisher _eventPublisher;
    private readonly ProcessReferralHandler _sut;

    public ProcessReferralHandlerTests()
    {
        _repository = Substitute.For<IUserReferralRepository>();
        _eventPublisher = Substitute.For<IIdentityEventPublisher>();
        var logger = Substitute.For<ILogger<ProcessReferralHandler>>();
        _sut = new ProcessReferralHandler(_repository, _eventPublisher, logger);
    }

    private static User CreateUser(long id, string email = "user@test.com") => new()
    {
        Id = id, Email = email, Name = "User", LastName = "Test"
    };

    // ═════════════════════════════════════════════════════════════════
    // Branch 1: No referrer → exits early, no DB call
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_NoReferrer_DoesNotCreateReferral()
    {
        var notification = new UserCreatedDomainEvent(CreateUser(1), referrer: null, referralCode: null);

        await _sut.Handle(notification, CancellationToken.None);

        await _repository.DidNotReceive().CreateUserReferralAsync(Arg.Any<UserReferral>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 2: Empty referral code → exits early
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_EmptyReferralCode_DoesNotCreateReferral()
    {
        var referrer = CreateUser(99);
        var notification = new UserCreatedDomainEvent(CreateUser(1), referrer, referralCode: "");

        await _sut.Handle(notification, CancellationToken.None);

        await _repository.DidNotReceive().CreateUserReferralAsync(Arg.Any<UserReferral>());
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 3: Valid referral → creates UserReferral with correct data
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidReferral_CreatesReferralWithCorrectData()
    {
        var newUser = CreateUser(1);
        var referrer = CreateUser(99, "referrer@test.com");
        const string code = "REF123";

        _repository.CreateUserReferralAsync(Arg.Any<UserReferral>())
            .Returns(args => args.Arg<UserReferral>());

        var notification = new UserCreatedDomainEvent(newUser, referrer, code);

        await _sut.Handle(notification, CancellationToken.None);

        await _repository.Received(1).CreateUserReferralAsync(
            Arg.Is<UserReferral>(r =>
                r.ReferrerId == referrer.Id &&
                r.ReferredId == newUser.Id &&
                r.UsedSecurityCode == code));
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 4: Valid referral → publishes event
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Handle_ValidReferral_PublishesReferralCreatedEvent()
    {
        var newUser = CreateUser(1);
        var referrer = CreateUser(99);
        const string code = "REF456";

        _repository.CreateUserReferralAsync(Arg.Any<UserReferral>())
            .Returns(args => args.Arg<UserReferral>());

        var notification = new UserCreatedDomainEvent(newUser, referrer, code);

        await _sut.Handle(notification, CancellationToken.None);

        // Wait briefly for fire-and-forget publish
        await Task.Delay(50);

        await _eventPublisher.Received(1).PublishReferralCreatedAsync(
            Arg.Is<User>(u => u.Id == referrer.Id),
            Arg.Is<User>(u => u.Id == newUser.Id),
            code);
    }

    // ═════════════════════════════════════════════════════════════════
    // Branch 5: Repository throws → does not rethrow (fire-and-forget)
    // ═════════════════════════════════════════════════════���═══════════

    [Fact]
    public async Task Handle_RepositoryThrows_DoesNotRethrow()
    {
        var newUser = CreateUser(1);
        var referrer = CreateUser(99);

        _repository.CreateUserReferralAsync(Arg.Any<UserReferral>())
            .ThrowsAsync(new Exception("DB error"));

        var notification = new UserCreatedDomainEvent(newUser, referrer, "CODE");

        // Should not throw
        var act = async () => await _sut.Handle(notification, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}

