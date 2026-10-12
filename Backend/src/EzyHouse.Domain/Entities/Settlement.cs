using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Entities;

public class Settlement : IBalanceContributor
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }

    public Household? Household { get; set; }

    public int FromUserId { get; set; }

    public User? From { get; set; }

    public int ToUserId { get; set; }

    public User? To { get; set; }

    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public int CreatedByUserId { get; set; }

    public User? CreatedBy { get; set; }

    /// <summary>
    /// When <see cref="FromUserId"/> pays <see cref="ToUserId"/>, the payment cancels
    /// the same amount of what <see cref="FromUserId"/> owed.
    /// </summary>
    public IEnumerable<LedgerEntry> GetLedgerEntries() =>
        [new LedgerEntry(DebtorId: ToUserId, CreditorId: FromUserId, Amount: Amount)];
}
