using EzyHouse.Api.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EzyHouse.Api.Features.Auth;

[ApiController]
[Route("api")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("auth/register")]
    public Task<AuthResultDto> Register(RegisterRequest request) => auth.RegisterAsync(request);

    [AllowAnonymous]
    [HttpPost("auth/login")]
    public Task<AuthResultDto> Login(LoginRequest request) => auth.LoginAsync(request);

    [HttpGet("me")]
    public Task<UserDto> Me() => auth.GetMeAsync();
}
