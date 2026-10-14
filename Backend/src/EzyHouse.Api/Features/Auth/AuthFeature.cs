using EzyHouse.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EzyHouse.Api.Features.Auth;

public static class AuthFeature
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
