using EzyHouse.Api.Core;
using EzyHouse.Api.Features.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EzyHouse.Tests.Features;

public class AuthControllerTests
{
    private const int ExpiryMinutes = 60;

    private DefaultHttpContext http = null!;
    private AuthController controller = null!;

    [SetUp]
    public void SetUp()
    {
        http = new DefaultHttpContext();
        http.Request.Scheme = "http";
        controller = new AuthController(new StubAuthService(), Options.Create(new JwtOptions { ExpiryMinutes = ExpiryMinutes }))
        {
            ControllerContext = new ControllerContext { HttpContext = http },
        };
    }

    [Test]
    public async Task Login_returns_the_token_and_sets_it_as_an_httponly_strict_cookie_lasting_as_long_as_the_token()
    {
        AuthResultDto result = await controller.Login(new LoginRequest("alice@example.com", "Password1!"));

        string cookie = SetCookie();
        Assert.Multiple(() =>
        {
            Assert.That(result.Token, Is.EqualTo(StubAuthService.Token));
            Assert.That(cookie, Does.StartWith($"ezyhouse_token={StubAuthService.Token};"));
            Assert.That(cookie, Does.Contain("httponly"));
            Assert.That(cookie, Does.Contain("samesite=strict"));
            Assert.That(cookie, Does.Contain("path=/"));
            Assert.That(cookie, Does.Contain($"max-age={ExpiryMinutes * 60}"));
            Assert.That(cookie, Does.Not.Contain("secure"));
        });
    }

    [Test]
    public async Task Register_also_sets_the_login_cookie()
    {
        await controller.Register(new RegisterRequest("Alice", "alice@example.com", "Password1!"));

        Assert.That(SetCookie(), Does.StartWith($"ezyhouse_token={StubAuthService.Token};"));
    }

    [Test]
    public async Task Over_https_the_login_cookie_is_secure()
    {
        http.Request.Scheme = "https";

        await controller.Login(new LoginRequest("alice@example.com", "Password1!"));

        Assert.That(SetCookie(), Does.Contain("secure"));
    }

    [Test]
    public void Logout_returns_204_and_expires_the_login_cookie()
    {
        IActionResult result = controller.Logout();

        string cookie = SetCookie();
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            Assert.That(cookie, Does.StartWith("ezyhouse_token=;"));
            Assert.That(cookie, Does.Contain("expires=Thu, 01 Jan 1970 00:00:00 GMT"));
            Assert.That(cookie, Does.Contain("path=/"));
            Assert.That(cookie, Does.Contain("samesite=strict"));
        });
    }

    private string SetCookie() => http.Response.Headers.SetCookie.Single()!;

    private sealed class StubAuthService : IAuthService
    {
        public const string Token = "header.payload.signature";

        private static readonly AuthResultDto Result =
            new(Token, new UserDto(1, "Alice", "alice@example.com", null));

        public Task<AuthResultDto> RegisterAsync(RegisterRequest request) => Task.FromResult(Result);

        public Task<AuthResultDto> LoginAsync(LoginRequest request) => Task.FromResult(Result);

        public Task<UserDto> GetMeAsync() => throw new NotSupportedException();
    }
}
