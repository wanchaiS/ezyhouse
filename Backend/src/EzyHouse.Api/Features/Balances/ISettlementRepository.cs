using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.Balances;

// Settlements can't be undone or edited, so there is no update or delete here.
public interface ISettlementRepository : IRepository
{
    Task<IReadOnlyList<Settlement>> ListAsync(int householdId);

    Task<IReadOnlyDictionary<int, User>> FindUsersAsync(IEnumerable<int> userIds);

    void Add(Settlement settlement);
}
