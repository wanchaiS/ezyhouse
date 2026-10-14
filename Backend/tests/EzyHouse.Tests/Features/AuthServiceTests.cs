using EzyHouse.Api.Core;
using EzyHouse.Api.Features.Auth;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EzyHouse.Tests.Features;

public class AuthServiceTests
{
    private TestDb testDb = null!;
    private EzyHouseDbContext db = null!;
    private AuthService auth = null!;

    [SetUp]
    public async Task SetUp()
    {
        testDb = await TestDb.CreateAsync();
        db = testDb.NewContext();
        var tokens = new JwtTokenService(Options.Create(new JwtOptions
        {
            Issuer = "ezyhouse",
            Audience = "ezyhouse-web",
            Key = "a-test-signing-key-of-at-least-32-characters",
            ExpiryMinutes = 60,
        }));
        auth = new AuthService(new UserRepository(db), new PasswordHasher<User>(), tokens, new NoCurrentUser());
    }

    [TearDown]
    public async Task TearDown()
    {
        await db.DisposeAsync();
        await testDb.DisposeAsync();
    }

    [Test]
    public async Task Registering_an_email_that_is_already_used_is_rejected_whatever_its_case()
    {
        await auth.RegisterAsync(new RegisterRequest("Alice", "alice@example.com", "Password1!"));

        Assert.ThrowsAsync<BusinessRuleException>(() =>
            auth.RegisterAsync(new RegisterRequest("Alice Again", " Alice@Example.COM ", "Password2!")));
    }

    [Test]
    public async Task A_wrong_password_and_an_unknown_email_fail_with_the_same_401_message()
    {
        await auth.RegisterAsync(new RegisterRequest("Alice", "alice@example.com", "Password1!"));

        var wrongPassword = Assert.ThrowsAsync<UnauthorizedException>(() =>
            auth.LoginAsync(new LoginRequest("alice@example.com", "not-the-password")));
        var unknownEmail = Assert.ThrowsAsync<UnauthorizedException>(() =>
            auth.LoginAsync(new LoginRequest("nobody@example.com", "Password1!")));

        Assert.That(wrongPassword!.Message, Is.EqualTo(unknownEmail!.Message));
    }

    [Test]
    public async Task Logging_in_returns_a_token_whose_subject_is_the_user_id()
    {
        AuthResultDto registered = await auth.RegisterAsync(new RegisterRequest("Alice", "alice@example.com", "Password1!"));

        AuthResultDto loggedIn = await auth.LoginAsync(new LoginRequest("ALICE@example.com", "Password1!"));

        Assert.That(new JsonWebToken(loggedIn.Token).Subject, Is.EqualTo(registered.User.Id.ToString()));
    }

    // Register and login never ask who is calling.
    private sealed class NoCurrentUser : ICurrentUserContext
    {
        public int UserId => throw new InvalidOperationException("Not used by these tests.");

        public Task<User> GetUserAsync() => throw new InvalidOperationException("Not used by these tests.");

        public Task<Household> RequireHouseholdAsync() => throw new InvalidOperationException("Not used by these tests.");

        public Task<Household> RequirePrimaryAsync() => throw new InvalidOperationException("Not used by these tests.");
    }
}
