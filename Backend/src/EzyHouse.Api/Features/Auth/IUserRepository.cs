using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.Auth;

public interface IUserRepository : IRepository
{
    Task<User?> FindByEmailAsync(string email);

    void Add(User user);
}
