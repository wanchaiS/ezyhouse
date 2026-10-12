using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Interfaces;

public interface IBalanceContributor
{
    IEnumerable<LedgerEntry> GetLedgerEntries();
}
