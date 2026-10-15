using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Entities;

/// <summary>
/// Every cost in the household. The subclass decides the tag and who may change the expense;
/// the money always counts the same way, whatever the tag.
/// </summary>
public abstract class Expense : IBalanceContributor, IHouseholdOwned
{
    protected Expense(ExpenseTag tag)
    {
        Tag = tag;
    }

    public int Id { get; set; }

    public int HouseholdId { get; set; }

    public Household? Household { get; set; }

    /// <summary>Also the database discriminator, so it is fixed by the subclass and never changes.</summary>
    public ExpenseTag Tag { get; private set; }

    public string Description { get; set; } = "";

    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public int PaidByUserId { get; set; }

    public User? PaidBy { get; set; }

    public int CreatedByUserId { get; set; }

    public User? CreatedBy { get; set; }

    public SplitType SplitType { get; set; }

    public List<Share> Shares { get; set; } = [];

    /// <summary>Whether this person may edit or delete the expense, given who is primary right now.</summary>
    public abstract bool CanBeChangedBy(int userId, int primaryUserId);

    /// <summary>Each share holder other than the payer owes the payer their share.</summary>
    public IEnumerable<LedgerEntry> GetLedgerEntries() =>
        Shares
            .Where(share => share.UserId != PaidByUserId)
            .Select(share => new LedgerEntry(share.UserId, PaidByUserId, share.Amount));

    /// <summary>A new, empty expense of the kind the tag names.</summary>
    public static Expense Create(ExpenseTag tag) => tag switch
    {
        ExpenseTag.Rent => new RentExpense(),
        ExpenseTag.Bills => new BillExpense(),
        ExpenseTag.Groceries => new GroceryExpense(),
        ExpenseTag.Furniture => new FurnitureExpense(),
        _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, "Unknown expense tag."),
    };
}

/// <summary>Rent is the primary's to manage, whoever created the expense.</summary>
public class RentExpense() : Expense(ExpenseTag.Rent)
{
    /// <summary>Set when a rent schedule created it; null once that schedule is deleted.</summary>
    public int? RentScheduleId { get; set; }

    /// <summary>The last day of the rent period, when a schedule created it.</summary>
    public DateOnly? PeriodEnd { get; set; }

    public override bool CanBeChangedBy(int userId, int primaryUserId) => userId == primaryUserId;
}

/// <summary>Bills are the primary's to manage: the primary enters each bill's real amount.</summary>
public class BillExpense() : Expense(ExpenseTag.Bills)
{
    /// <summary>Set when a bill type created it; null once that bill type is deleted.</summary>
    public int? BillTypeId { get; set; }

    public override bool CanBeChangedBy(int userId, int primaryUserId) => userId == primaryUserId;
}

/// <summary>Groceries belong to whoever added them, so a housemate's shopping can't be rewritten by others.</summary>
public class GroceryExpense() : Expense(ExpenseTag.Groceries)
{
    public override bool CanBeChangedBy(int userId, int primaryUserId) => userId == CreatedByUserId;
}

/// <summary>Furniture is shared household property, so only the primary manages it.</summary>
public class FurnitureExpense() : Expense(ExpenseTag.Furniture)
{
    public override bool CanBeChangedBy(int userId, int primaryUserId) => userId == primaryUserId;
}
