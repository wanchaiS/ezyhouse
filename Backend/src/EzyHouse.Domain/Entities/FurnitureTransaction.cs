using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Entities;

public abstract class FurnitureTransaction : IBalanceContributor
{
    public int Id { get; set; }

    public int FurnitureId { get; set; }

    public Furniture? Furniture { get; set; }

    public DateOnly Date { get; set; }

    public decimal Amount { get; set; }

    /// <summary>
    /// The person who paid for a purchase or disposal, or who received the cash for a sale.
    /// </summary>
    public int CounterpartyUserId { get; set; }

    public User? Counterparty { get; set; }

    public SplitType SplitType { get; set; }

    public List<Share> Shares { get; set; } = [];

    public abstract IEnumerable<LedgerEntry> GetLedgerEntries();
}

public class FurniturePurchase : FurnitureTransaction
{
    /// <summary>Share holders owe the person who paid.</summary>
    public override IEnumerable<LedgerEntry> GetLedgerEntries() =>
        Shares
            .Where(share => share.UserId != CounterpartyUserId)
            .Select(share => new LedgerEntry(share.UserId, CounterpartyUserId, share.Amount));
}

public class FurnitureDisposal : FurnitureTransaction
{
    /// <summary>Share holders owe the person who paid.</summary>
    public override IEnumerable<LedgerEntry> GetLedgerEntries() =>
        Shares
            .Where(share => share.UserId != CounterpartyUserId)
            .Select(share => new LedgerEntry(share.UserId, CounterpartyUserId, share.Amount));
}

public class FurnitureSale : FurnitureTransaction
{
    /// <summary>
    /// The person who received the cash owes every other share holder their share.
    /// </summary>
    public override IEnumerable<LedgerEntry> GetLedgerEntries() =>
        Shares
            .Where(share => share.UserId != CounterpartyUserId)
            .Select(share => new LedgerEntry(CounterpartyUserId, share.UserId, share.Amount));
}
