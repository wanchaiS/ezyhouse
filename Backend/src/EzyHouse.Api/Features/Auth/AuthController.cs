using EzyHouse.Api.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EzyHouse.Api.Features.Auth;

[ApiController]
[Route("api")]
public sealed class AuthController(IAuthService auth, IOptions<JwtOptions> jwt) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("auth/register")]
    public async Task<AuthResultDto> Register(RegisterRequest request) =>
        SetCookie(await auth.RegisterAsync(request));

    [AllowAnonymous]
    [HttpPost("auth/login")]
    public async Task<AuthResultDto> Login(LoginRequest request) =>
        SetCookie(await auth.LoginAsync(request));

    [AllowAnonymous]
    [HttpPost("auth/logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookie.Name, AuthCookie.Options(Request));
        return NoContent();
    }

    [HttpGet("me")]
    public Task<UserDto> Me() => auth.GetMeAsync();

    private AuthResultDto SetCookie(AuthResultDto result)
    {
        Response.Cookies.Append(AuthCookie.Name, result.Token,
            AuthCookie.Options(Request, TimeSpan.FromMinutes(jwt.Value.ExpiryMinutes)));
        return result;
    }
}
