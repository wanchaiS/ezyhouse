using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Features.Auth;

public interface ITokenService
{
    string CreateToken(User user);
}
