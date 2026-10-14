using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Extensions;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Features.Balances;

public sealed class SettlementRepository(EzyHouseDbContext db) : ISettlementRepository
{
    public async Task<IReadOnlyList<Settlement>> ListAsync(int householdId) =>
        await db.Settlements
            .ForHousehold(householdId)
            .Include(settlement => settlement.From)
            .Include(settlement => settlement.To)
            .Include(settlement => settlement.CreatedBy)
            .OrderByDescending(settlement => settlement.Date)
            .ThenByDescending(settlement => settlement.Id)
            .ToListAsync();

    public async Task<IReadOnlyDictionary<int, User>> FindUsersAsync(IEnumerable<int> userIds)
    {
        List<int> ids = userIds.Distinct().ToList();
        return await db.Users.Where(user => ids.Contains(user.Id)).ToDictionaryAsync(user => user.Id);
    }

    public void Add(Settlement settlement) => db.Settlements.Add(settlement);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
