using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.Bonds;

public interface IBondRepository : IRepository
{
    Task<List<Bond>> ListAsync(int householdId);

    Task<Bond?> FindAsync(int householdId, int id);

    void Add(Bond bond);

    void Remove(Bond bond);
}
