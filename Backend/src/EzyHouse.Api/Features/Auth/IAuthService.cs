using EzyHouse.Api.Core;

namespace EzyHouse.Api.Features.Auth;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterRequest request);

    Task<AuthResultDto> LoginAsync(LoginRequest request);

    Task<UserDto> GetMeAsync();
}
