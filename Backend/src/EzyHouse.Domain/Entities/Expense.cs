using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Entities;

public class Expense : IBalanceContributor
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }

    public Household? Household { get; set; }

    public string Description { get; set; } = "";

    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public int PaidByUserId { get; set; }

    public User? PaidBy { get; set; }

    public int CreatedByUserId { get; set; }

    public User? CreatedBy { get; set; }

    public SplitType SplitType { get; set; }

    public List<Share> Shares { get; set; } = [];

    /// <summary>Each share holder other than the payer owes the payer their share.</summary>
    public IEnumerable<LedgerEntry> GetLedgerEntries() =>
        Shares
            .Where(share => share.UserId != PaidByUserId)
            .Select(share => new LedgerEntry(share.UserId, PaidByUserId, share.Amount));
}
