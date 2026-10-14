using System.Text;
using EzyHouse.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EzyHouse.Api.Features.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    public string CreateToken(User user)
    {
        JwtOptions jwt = options.Value;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Expires = DateTime.UtcNow.AddMinutes(jwt.ExpiryMinutes),
            // "sub" carries the user id; the API reads it back through ClaimsPrincipalExtensions.GetUserId
            Claims = new Dictionary<string, object>
            {
                ["sub"] = user.Id.ToString(),
                ["name"] = user.Name,
                ["email"] = user.Email,
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), SecurityAlgorithms.HmacSha256),
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
