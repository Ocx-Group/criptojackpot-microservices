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
/// Tests de integración para el flujo de autenticación.
/// Usa Testcontainers (PostgreSQL + Redpanda) para probar el stack real.
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

    // ─── Lifecycle: seed datos base y crear cliente fresco por test ──

    public async Task InitializeAsync()
    {
        // Cliente fresco para cada test (cookies limpias)
        _client = _factory.CreateClientWithCookies();
        await DatabaseSeeder.SeedBaseDataAsync(_factory.Services);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await DatabaseSeeder.CleanUsersAsync(_factory.Services);
    }

    // ═════════════════════════════════════════════════════════════════
    // 1. Login exitoso → 200 + HttpOnly cookies + UserLoggedInEvent
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_ValidCredentials_Returns200_SetsCookies_PublishesEvent()
    {
        // Arrange — usuario real en PostgreSQL
        var (user, plainPassword) = await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "success@test.com",
            password: "ValidPass123!");

        var payload = new { email = user.Email, password = plainPassword, rememberMe = false };

        // Act — HTTP POST real al endpoint
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        // Assert — HTTP Response
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginApiResponse>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.Email.Should().Be(user.Email);
        body.Data.RequiresTwoFactor.Should().BeFalse();

        // Assert — HttpOnly Cookies (BFF pattern)
        // El servidor debe setear access_token y refresh_token como HttpOnly cookies
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        var cookieList = cookies!.ToList();
        cookieList.Should().Contain(c => c.Contains("access_token="));
        cookieList.Should().Contain(c => c.Contains("refresh_token="));
        cookieList.Should().Contain(c => c.Contains("httponly", StringComparison.OrdinalIgnoreCase));

        // Assert — Evento publicado a Kafka (capturado por MassTransit TestHarness)
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        (await harness.Published.Any<UserLoggedInEvent>()).Should().BeTrue(
            "LoginCommandHandler debe publicar UserLoggedInEvent via IEventBus al completar login");
    }

    // ═════════════════════════════════════════════════════════════════
    // 2. Login con credenciales inválidas → 401 + sin cookies
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_InvalidPassword_Returns401_NoCookies()
    {
        // Arrange
        await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "wrong@test.com",
            password: "CorrectPass123!");

        var payload = new { email = "wrong@test.com", password = "WrongPassword!", rememberMe = false };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
        body!.Success.Should().BeFalse();
        body.Message.Should().Be("Invalid email or password");

        // Sin cookies — el servidor no debe setear tokens
        response.Headers.Contains("Set-Cookie").Should().BeFalse();
    }

    // ═════════════════════════════════════════════════════════════════
    // 3. Login con email no verificado → 403
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_EmailNotVerified_Returns403()
    {
        // Arrange
        var (user, plainPassword) = await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "unverified@test.com",
            password: "TestPass123!",
            emailVerified: false);

        var payload = new { email = user.Email, password = plainPassword, rememberMe = false };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var body = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
        body!.Message.Should().Contain("verify your email");
    }

    // ═════════════════════════════════════════════════════════════════
    // 4. Login con cuenta Google-only → 401 con mensaje específico
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_GoogleOnlyAccount_Returns401_WithGoogleMessage()
    {
        // Arrange
        var user = await DatabaseSeeder.CreateGoogleOnlyUserAsync(
            _factory.Services,
            email: "googleonly@test.com");

        var payload = new { email = user.Email, password = "AnyPassword!", rememberMe = false };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
        body!.Message.Should().Contain("Google sign-in");
    }

    // ═════════════════════════════════════════════════════════════════
    // 5. Lockout progresivo: 3 intentos fallidos → 423 Locked
    //    Verifica que el estado persiste en PostgreSQL entre requests
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_ThreeFailedAttempts_Returns423Locked_PublishesLockoutEvent()
    {
        // Arrange
        await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "lockout@test.com",
            password: "CorrectPass123!");

        var wrongPayload = new { email = "lockout@test.com", password = "WrongPass!", rememberMe = false };

        // Act — 3 intentos fallidos consecutivos
        // Cada request persiste FailedLoginAttempts en PostgreSQL real
        var response1 = await _client.PostAsJsonAsync("/api/v1/auth/login", wrongPayload);
        var response2 = await _client.PostAsJsonAsync("/api/v1/auth/login", wrongPayload);
        var response3 = await _client.PostAsJsonAsync("/api/v1/auth/login", wrongPayload);

        // Assert — primeros 2 intentos: 401 (Unauthorized)
        response1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Tercer intento: 423 (Locked) — lockout progresivo activado
        // HttpStatusCode.Locked = 423
        ((int)response3.StatusCode).Should().Be(423);

        var body = await response3.Content.ReadFromJsonAsync<LockedApiResponse>();
        body!.RetryAfterSeconds.Should().BeGreaterThan(0);

        // Assert — UserLockedOutEvent publicado
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        (await harness.Published.Any<UserLockedOutEvent>()).Should().BeTrue(
            "Después de 3 intentos fallidos, debe publicar UserLockedOutEvent");

        // Assert — siguiente intento con password correcto también falla (cuenta bloqueada)
        var correctPayload = new { email = "lockout@test.com", password = "CorrectPass123!", rememberMe = false };
        var response4 = await _client.PostAsJsonAsync("/api/v1/auth/login", correctPayload);
        ((int)response4.StatusCode).Should().Be(423, "la cuenta debe permanecer bloqueada");
    }

    // ═════════════════════════════════════════════════════════════════
    // 6. Login → Refresh → verifica que token rotation funciona
    //    End-to-end: login setea cookies → refresh las rota
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_ThenRefresh_RotatesTokens_InCookies()
    {
        // Arrange
        var (user, plainPassword) = await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "refresh@test.com",
            password: "RefreshTest123!");

        var loginPayload = new { email = user.Email, password = plainPassword, rememberMe = false };

        // Act — Login (setea cookies)
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act — Refresh (las cookies se envían automáticamente via CookieContainerHandler)
        var refreshResponse = await _client.PostAsync("/api/v1/auth/refresh", null);

        // Assert — Refresh exitoso
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert — Nuevas cookies seteadas (token rotation)
        refreshResponse.Headers.TryGetValues("Set-Cookie", out var newCookies).Should().BeTrue();
        var newCookieList = newCookies!.ToList();
        newCookieList.Should().Contain(c => c.Contains("access_token="));
        newCookieList.Should().Contain(c => c.Contains("refresh_token="));
    }

    // ═════════════════════════════════════════════════════════════════
    // 7. Login → Logout → Refresh falla (token revocado)
    //    Verifica el flujo completo de sesión
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_Logout_ThenRefresh_Returns401()
    {
        // Arrange
        var (user, plainPassword) = await DatabaseSeeder.CreateVerifiedUserAsync(
            _factory.Services,
            email: "logout@test.com",
            password: "LogoutTest123!");

        var loginPayload = new { email = user.Email, password = plainPassword, rememberMe = false };

        // Act — Login
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act — Logout (revoca refresh token)
        var logoutResponse = await _client.PostAsync("/api/v1/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act — Refresh con token revocado
        var refreshResponse = await _client.PostAsync("/api/v1/auth/refresh", null);

        // Assert — debe fallar porque el token fue revocado
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ═════════════════════════════════════════════════════════════════
    // 8. Usuario no existente → 401 (no leak de info)
    // ═════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Login_NonExistentUser_Returns401_SameMessageAsWrongPassword()
    {
        // Arrange — no creamos usuario
        var payload = new { email = "ghost@test.com", password = "AnyPass123!", rememberMe = false };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

        // Assert — mismo mensaje que password incorrecto (no user enumeration)
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var body = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
        body!.Message.Should().Be("Invalid email or password");
    }

    // ─── DTOs para deserializar responses ────────────────────────

    private record LoginApiResponse(bool Success, UserDataResponse? Data);
    private record UserDataResponse(
        string Email,
        string Name,
        string LastName,
        bool RequiresTwoFactor,
        bool TwoFactorEnabled);
    private record ErrorApiResponse(bool Success, string Message);
    private record LockedApiResponse(bool Success, string Message, int RetryAfterSeconds); 
}