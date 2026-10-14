using EzyHouse.Domain.Entities;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Features.Auth;

public sealed class UserRepository(EzyHouseDbContext db) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string email) =>
        db.Users.FirstOrDefaultAsync(user => user.Email == email);

    public void Add(User user) => db.Users.Add(user);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
