using System.Net;
using System.Net.Http.Json;
using CryptoJackpot.Domain.Core.IntegrationEvents.Identity;
using CryptoJackpot.Identity.IntegrationTests.Infrastructure;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CryptoJackpot.Identity.IntegrationTests.Auth;

/// <summary>
/// Integration tests para el flujo de Login.
///
/// CORRECCIONES:
///   - SeedBaseDataAsync REMOVIDO de InitializeAsync (ya se ejecuta en la Factory)
///   - CleanUsersAsync usa ExecuteDeleteAsync (bulk DELETE, sin SELECT previo)
///   - Cada test solo crea su usuario específico → mínimo I/O
/// </summary>
[Collection(nameof(IdentityApiCollection))]
public class LoginFlowTests : IAsyncLifetime
{
    private readonly IdentityApiFactory _factory;
    private HttpClient _client = null!;

    public LoginFlowTests(IdentityApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        // Solo crear cliente fresco (cookies limpias por test)
        // SeedBaseDataAsync ya se ejecutó UNA vez en la Factory
        _client = _factory.CreateClientWithCookies();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        // ExecuteDeleteAsync: un DELETE FROM por tabla, sin cargar entidades
        await DatabaseSeeder.CleanUsersAsync(_factory.Services);
    }

    // ═════════════════════════════════════════════════════════════════
    // 1. Login exitoso → 200 + HttpOnly cookies + UserLoggedInEvent
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_ValidCredentials_Returns200_SetsCookies_PublishesEvent()
    {
        var (user, plainPassword) = await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "success@test.com",
            password: "ValidPass123!");

        var payload = new { email = user.Email, password = plainPassword, rememberMe = false };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginApiResponse>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.Email.Should().Be(user.Email);
        body.Data.RequiresTwoFactor.Should().BeFalse();

        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        var cookieList = cookies!.ToList();
        cookieList.Should().Contain(c => c.Contains("access_token="));
        cookieList.Should().Contain(c => c.Contains("refresh_token="));
        cookieList.Should().Contain(c => c.Contains("httponly", StringComparison.OrdinalIgnoreCase));

        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        (await harness.Published.Any<UserLoggedInEvent>()).Should().BeTrue(
            "LoginCommandHandler debe publicar UserLoggedInEvent via IEventBus");
    }

    // ═════════════════════════════════════════════════════════════════
    // 2. Login con credenciales inválidas → 401 + sin cookies
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_InvalidPassword_Returns401_NoCookies()
    {
        await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "wrong@test.com",
            password: "CorrectPass123!");

        var payload = new { email = "wrong@test.com", password = "WrongPassword!", rememberMe = false };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
        body!.Success.Should().BeFalse();
        body.Message.Should().Be("Invalid email or password");

        response.Headers.Contains("Set-Cookie").Should().BeFalse();
    }

    // ═════════════════════════════════════════════════════════════════
    // 3. Email no verificado → 403
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_EmailNotVerified_Returns403()
    {
        var (user, plainPassword) = await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "unverified@test.com",
            password: "TestPass123!",
            emailVerified: false);

        var payload = new { email = user.Email, password = plainPassword, rememberMe = false };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var body = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
        body!.Message.Should().Contain("verify your email");
    }

    // ═════════════════════════════════════════════════════════════════
    // 4. Cuenta Google-only → 401 con mensaje específico
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_GoogleOnlyAccount_Returns401_WithGoogleMessage()
    {
        var user = await DatabaseSeeder.CreateGoogleOnlyUserAsync(
            _factory.Services,
            email: "googleonly@test.com");

        var payload = new { email = user.Email, password = "AnyPassword!", rememberMe = false };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
        body!.Message.Should().Contain("Google sign-in");
    }

    // ═════════════════════════════════════════════════════════════════
    // 5. Lockout progresivo: 3 intentos fallidos → 423
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_ThreeFailedAttempts_Returns423Locked_PublishesLockoutEvent()
    {
        await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "lockout@test.com",
            password: "CorrectPass123!");

        var wrongPayload = new { email = "lockout@test.com", password = "WrongPass!", rememberMe = false };

        var response1 = await _client.PostAsJsonAsync("/api/v1/auth/login", wrongPayload);
        var response2 = await _client.PostAsJsonAsync("/api/v1/auth/login", wrongPayload);
        var response3 = await _client.PostAsJsonAsync("/api/v1/auth/login", wrongPayload);

        response1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        ((int)response3.StatusCode).Should().Be(423);

        var body = await response3.Content.ReadFromJsonAsync<LockedApiResponse>();
        body!.RetryAfterSeconds.Should().BeGreaterThan(0);

        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        (await harness.Published.Any<UserLockedOutEvent>()).Should().BeTrue(
            "Después de 3 intentos fallidos, debe publicar UserLockedOutEvent");

        // Cuenta sigue bloqueada incluso con password correcto
        var correctPayload = new { email = "lockout@test.com", password = "CorrectPass123!", rememberMe = false };
        var response4 = await _client.PostAsJsonAsync("/api/v1/auth/login", correctPayload);
        ((int)response4.StatusCode).Should().Be(423);
    }

    // ═════════════════════════════════════════════════════════════════
    // 6. Login → Refresh → token rotation
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_ThenRefresh_RotatesTokens_InCookies()
    {
        var (user, plainPassword) = await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "refresh@test.com",
            password: "RefreshTest123!");

        var loginPayload = new { email = user.Email, password = plainPassword, rememberMe = false };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponse = await _client.PostAsync("/api/v1/auth/refresh", null);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        refreshResponse.Headers.TryGetValues("Set-Cookie", out var newCookies).Should().BeTrue();
        var newCookieList = newCookies!.ToList();
        newCookieList.Should().Contain(c => c.Contains("access_token="));
        newCookieList.Should().Contain(c => c.Contains("refresh_token="));
    }

    // ═════════════════════════════════════════════════════════════════
    // 7. Login → Logout → Refresh falla
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_Logout_ThenRefresh_Returns401()
    {
        var (user, plainPassword) = await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "logout@test.com",
            password: "LogoutTest123!");

        var loginPayload = new { email = user.Email, password = plainPassword, rememberMe = false };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var logoutResponse = await _client.PostAsync("/api/v1/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshResponse = await _client.PostAsync("/api/v1/auth/refresh", null);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ═════════════════════════════════════════════════════════════════
    // 8. Usuario inexistente → 401 (anti user-enumeration)
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_NonExistentUser_Returns401_SameMessageAsWrongPassword()
    {
        var payload = new { email = "ghost@test.com", password = "AnyPass123!", rememberMe = false };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
        body!.Message.Should().Be("Invalid email or password");
    }

    // ─── Response DTOs ───────────────────────────────────────────

    private record LoginApiResponse(bool Success, UserDataResponse? Data);
    private record UserDataResponse(string Email, bool RequiresTwoFactor);
    private record ErrorApiResponse(bool Success, string Message);
    private record LockedApiResponse(bool Success, string Message, int RetryAfterSeconds);
}