using EzyHouse.Api.Dtos;

namespace EzyHouse.Api.Services;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterRequest request);

    Task<AuthResultDto> LoginAsync(LoginRequest request);

    Task<UserDto> GetMeAsync();
}
