using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Scheduling;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Entities;

public class RentSchedule : RecurringSchedule<RentPeriod>
{
    public decimal Amount { get; set; }

    public SplitType SplitType { get; set; }

    public List<Share> Shares { get; set; } = [];

    public List<RentPeriod> Periods { get; set; } = [];

    /// <summary>
    /// The payee is whoever is primary now. The payee's own share is ticked automatically.
    /// </summary>
    public override RentPeriod CreateOccurrence(ScheduleSlot slot, int primaryUserId) => new()
    {
        ScheduleId = Id,
        StartDate = slot.Start,
        EndDate = slot.End,
        Amount = Amount,
        PayeeUserId = primaryUserId,
        Shares = Shares.Select(share => new PaidShare(share.UserId, share.Amount)
        {
            Paid = share.UserId == primaryUserId,
            PaidDate = share.UserId == primaryUserId ? slot.Start : null,
        }).ToList(),
    };
}
