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
            Frequency = Frequency.Monthly,
            StartDate = new DateOnly(2026, 2, 1),
            Amount = 90m,
            Shares = [new Share(1, 30m), new Share(2, 30m), new Share(3, 30m)],
        };

        RentPeriod period = schedule.GenerateMissing(null, new DateOnly(2026, 2, 10), primaryUserId: 2).Single();

        Assert.That(period.PayeeUserId, Is.EqualTo(2));
        Assert.That(period.Shares.Select(share => (share.UserId, share.Amount, share.Paid, share.PaidDate)), Is.EqualTo(new (int, decimal, bool, DateOnly?)[]
        {
            (1, 30m, false, null),
            (2, 30m, true, new DateOnly(2026, 2, 1)),
            (3, 30m, false, null),
        }));
    }

    [Test]
    public void Generating_only_creates_slots_after_the_last_covered_date()
    {
        var schedule = new RentSchedule
        {
            Frequency = Frequency.Weekly,
            StartDate = new DateOnly(2026, 1, 1),
        };

        var periods = schedule.GenerateMissing(new DateOnly(2026, 1, 8), new DateOnly(2026, 1, 20), primaryUserId: 1);

        Assert.That(periods.Select(period => period.StartDate), Is.EqualTo(new[] { new DateOnly(2026, 1, 15) }));
    }

    [Test]
    public void Moving_the_start_date_back_does_not_create_anything_when_already_covered_past_today()
    {
        var schedule = new RentSchedule
        {
            Frequency = Frequency.Weekly,
            StartDate = new DateOnly(2025, 11, 1),
        };

        var periods = schedule.GenerateMissing(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 20), primaryUserId: 1);

        Assert.That(periods, Is.Empty);
    }

    [Test]
    public void Next_due_date_for_a_schedule_starting_more_than_4_months_out_is_its_start_date()
    {
        var schedule = new RentSchedule
        {
            Frequency = Frequency.Monthly,
            StartDate = new DateOnly(2026, 12, 1),
        };

        Assert.That(schedule.GetNextDueDate(new DateOnly(2026, 1, 1)), Is.EqualTo(new DateOnly(2026, 12, 1)));
    }
}
