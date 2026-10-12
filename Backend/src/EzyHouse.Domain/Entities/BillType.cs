using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Scheduling;

namespace EzyHouse.Domain.Entities;

public class BillType : RecurringSchedule<Bill>
{
    public string Name { get; set; } = "";

    public List<User> Members { get; set; } = [];

    public List<Bill> Bills { get; set; } = [];

    /// <summary>A placeholder has no amount, payee or shares until the primary submits it.</summary>
    public override Bill CreateOccurrence(ScheduleSlot slot, int primaryUserId) => new()
    {
        HouseholdId = HouseholdId,
        BillTypeId = Id,
        BillTypeName = Name,
        DueDate = slot.Start,
        Status = BillStatus.Pending,
    };
}
