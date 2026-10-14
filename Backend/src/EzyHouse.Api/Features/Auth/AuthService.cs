using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace EzyHouse.Api.Features.Auth;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher<User> passwordHasher,
    ITokenService tokens,
    ICurrentUserContext currentUser) : IAuthService
{
    // The same message for an unknown email and a wrong password, so the API doesn't reveal which emails exist.
    private const string LoginFailed = "Email or password is incorrect.";

    public async Task<AuthResultDto> RegisterAsync(RegisterRequest request)
    {
        string email = NormaliseEmail(request.Email!);
        if (await users.FindByEmailAsync(email) is not null)
            throw new BusinessRuleException("An account with that email already exists.");

        var user = new User { Name = request.Name!.Trim(), Email = email };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password!);
        users.Add(user);
        await users.SaveChangesAsync();

        return new AuthResultDto(tokens.CreateToken(user), user.ToDto());
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequest request)
    {
        User? user = await users.FindByEmailAsync(NormaliseEmail(request.Email!));
        if (user is null ||
            passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password!) == PasswordVerificationResult.Failed)
            throw new UnauthorizedException(LoginFailed);

        return new AuthResultDto(tokens.CreateToken(user), user.ToDto());
    }

    public async Task<UserDto> GetMeAsync() => (await currentUser.GetUserAsync()).ToDto();

    private static string NormaliseEmail(string email) => email.Trim().ToLowerInvariant();
}
