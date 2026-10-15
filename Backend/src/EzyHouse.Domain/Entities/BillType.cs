using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Scheduling;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Entities;

public class BillType : RecurringSchedule<BillExpense>
{
    public string Name { get; set; } = "";

    public List<User> Members { get; set; } = [];

    public override bool HasAnyoneToShare => Members.Count > 0;

    /// <summary>
    /// The real amount isn't known until the bill arrives, so each bill starts at $0, split equally
    /// among the members (ordered by id, so the split is the same every time). The primary then
    /// edits it to the real amount.
    /// </summary>
    protected override BillExpense CreateOccurrence(ScheduleSlot slot, int primaryUserId) => new()
    {
        HouseholdId = HouseholdId,
        BillTypeId = Id,
        Description = Name,
        Amount = 0m,
        Date = slot.Start,
        PaidByUserId = primaryUserId,
        CreatedByUserId = primaryUserId,
        SplitType = SplitType.Equal,
        Shares = Members.OrderBy(member => member.Id).Select(member => new Share(member.Id, 0m)).ToList(),
    };
}
