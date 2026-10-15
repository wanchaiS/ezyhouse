using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Scheduling;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Entities;

public class RentSchedule : RecurringSchedule<RentExpense>
{
    public decimal Amount { get; set; }

    public SplitType SplitType { get; set; }

    public List<Share> Shares { get; set; } = [];

    public override bool HasAnyoneToShare => Shares.Count > 0;

    /// <summary>
    /// One rent expense per period, dated its first day, split as the schedule says. The shares are
    /// copied, so editing the schedule later never changes rent already created.
    /// </summary>
    protected override RentExpense CreateOccurrence(ScheduleSlot slot, int primaryUserId) => new()
    {
        HouseholdId = HouseholdId,
        RentScheduleId = Id,
        Description = "Rent",
        Amount = Amount,
        Date = slot.Start,
        PeriodEnd = slot.End,
        PaidByUserId = primaryUserId,
        CreatedByUserId = primaryUserId,
        SplitType = SplitType,
        Shares = Shares.Select(share => new Share(share.UserId, share.Amount)).ToList(),
    };
}
