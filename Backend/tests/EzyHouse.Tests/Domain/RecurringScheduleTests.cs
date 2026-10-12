using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Tests.Domain;

public class RecurringScheduleTests
{
    [Test]
    public void Monthly_schedule_starting_on_31_January_does_not_drift()
    {
        var schedule = new RentSchedule
        {
            Frequency = Frequency.Monthly,
            StartDate = new DateOnly(2026, 1, 31),
        };

        DateOnly[] starts = schedule.GetSlots(new DateOnly(2026, 3, 31)).Select(slot => slot.Start).ToArray();

        Assert.That(starts, Is.EqualTo(new[]
        {
            new DateOnly(2026, 1, 31),
            new DateOnly(2026, 2, 28),
            new DateOnly(2026, 3, 31),
        }));
    }

    [Test]
    public void End_date_stops_the_schedule_and_cuts_the_last_slot_back()
    {
        var schedule = new RentSchedule
        {
            Frequency = Frequency.Weekly,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 10),
        };

        var slots = schedule.GetSlots(new DateOnly(2026, 3, 1)).ToList();

        Assert.That(slots.Select(slot => slot.Start), Is.EqualTo(new[]
        {
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 8),
        }));
        Assert.That(slots[^1].End, Is.EqualTo(new DateOnly(2026, 1, 10)));
    }

    [Test]
    public void Next_due_date_is_null_after_the_schedule_has_ended()
    {
        var schedule = new RentSchedule
        {
            Frequency = Frequency.Monthly,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 31),
        };

        Assert.That(schedule.GetNextDueDate(new DateOnly(2026, 6, 1)), Is.Null);
    }

    [Test]
    public void Creating_a_rent_period_copies_shares_and_ticks_only_the_payee()
    {
        var schedule = new RentSchedule
        {
            Id = 7,
            Amount = 90m,
            Shares = [new Share(1, 30m), new Share(2, 30m), new Share(3, 30m)],
        };
        var slot = new EzyHouse.Domain.Scheduling.ScheduleSlot(0, new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28));

        RentPeriod period = schedule.CreateOccurrence(slot, primaryUserId: 2);

        Assert.That(period.PayeeUserId, Is.EqualTo(2));
        Assert.That(period.Shares.Select(share => (share.UserId, share.Amount, share.Paid, share.PaidDate)), Is.EqualTo(new (int, decimal, bool, DateOnly?)[]
        {
            (1, 30m, false, null),
            (2, 30m, true, new DateOnly(2026, 2, 1)),
            (3, 30m, false, null),
        }));
    }
}
