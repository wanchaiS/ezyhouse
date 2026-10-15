using EzyHouse.Domain.Balances;
using EzyHouse.Domain.Extensions;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Core;

public sealed class HouseholdBalances(EzyHouseDbContext db, BalanceCalculator calculator) : IHouseholdBalances
{
    public async Task<(IReadOnlyDictionary<int, decimal> Nets, IReadOnlyList<Debt> Debts)> CalculateAsync(
        int householdId,
        IEnumerable<int> memberIds)
    {
        // Balances are never stored: they are worked out from every expense, whatever its tag,
        // and every settlement, every time.
        List<IBalanceContributor> contributors = [];
        contributors.AddRange(await db.Expenses.AsNoTracking().ForHousehold(householdId).ToListAsync());
        contributors.AddRange(await db.Settlements.AsNoTracking().ForHousehold(householdId).ToListAsync());

        return calculator.Calculate(contributors, memberIds);
    }
}
