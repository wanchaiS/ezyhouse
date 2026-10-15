using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Core;

/// <summary>
/// Nets and pairwise debts for one household, from all its expenses (rent, bills, groceries and
/// furniture alike) and settlements. The bond is never included (API.md). Used by Balances and by the leave check.
/// </summary>
public interface IHouseholdBalances
{
    Task<(IReadOnlyDictionary<int, decimal> Nets, IReadOnlyList<Debt> Debts)> CalculateAsync(
        int householdId,
        IEnumerable<int> memberIds);
}
