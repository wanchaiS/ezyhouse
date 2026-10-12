using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Balances;

public sealed class BalanceCalculator
{
    public (IReadOnlyDictionary<int, decimal> Nets, IReadOnlyList<Debt> Debts) Calculate(
        IEnumerable<IBalanceContributor> contributors,
        IEnumerable<int> memberIds)
    {
        List<LedgerEntry> entries = contributors
            .SelectMany(contributor => contributor.GetLedgerEntries())
            .Where(entry => entry.DebtorId != entry.CreditorId)
            .ToList();

        // Net per member: positive means the household owes you, negative means you owe.
        var nets = memberIds.ToDictionary(
            id => id,
            id => entries.Where(entry => entry.CreditorId == id).Sum(entry => entry.Amount)
                - entries.Where(entry => entry.DebtorId == id).Sum(entry => entry.Amount));

        // Pairwise debts with no simplification: net each pair of people separately.
        var debts = entries
            .GroupBy(entry => (
                Low: Math.Min(entry.DebtorId, entry.CreditorId),
                High: Math.Max(entry.DebtorId, entry.CreditorId)))
            .Select(group => new
            {
                group.Key,
                Net = group.Sum(entry => entry.DebtorId == group.Key.Low ? entry.Amount : -entry.Amount),
            })
            .Where(pair => pair.Net != 0)
            .Select(pair => pair.Net > 0
                ? new Debt(pair.Key.Low, pair.Key.High, pair.Net)
                : new Debt(pair.Key.High, pair.Key.Low, -pair.Net))
            .ToList();

        return (nets, debts);
    }
}
