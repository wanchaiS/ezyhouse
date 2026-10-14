using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Core;

/// <summary>
/// Nets and pairwise debts for one household, from its expenses, furniture and settlements.
/// Rent, bills and bond are excluded (API.md). Used by Balances and by the leave check.
/// </summary>
public interface IHouseholdBalances
{
    Task<(IReadOnlyDictionary<int, decimal> Nets, IReadOnlyList<Debt> Debts)> CalculateAsync(
        int householdId,
        IEnumerable<int> memberIds);
}
