using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Tests.Domain;

public class RecurringScheduleTests
{
    private static readonly DateOnly January1 = new(2026, 1, 1);

    [Test]
    public void Monthly_schedule_starting_on_31_January_does_not_drift()
    {
        var schedule = new RentSchedule { Frequency = Frequency.Monthly, StartDate = new DateOnly(2026, 1, 31) };

        DateOnly[] starts = schedule.GetSlots().Take(3).Select(slot => slot.Start).ToArray();

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
        var schedule = new RentSchedule { Frequency = Frequency.Weekly, StartDate = January1, EndDate = new DateOnly(2026, 1, 10) };

        var slots = schedule.GetSlots().ToList();

        Assert.That(slots.Select(slot => slot.Start), Is.EqualTo(new[] { January1, new DateOnly(2026, 1, 8) }));
        Assert.That(slots[^1].End, Is.EqualTo(new DateOnly(2026, 1, 10)));
    }

    [Test]
    public void Beginning_a_schedule_that_started_in_the_past_skips_to_its_first_date_on_or_after_today()
    {
        var schedule = new RentSchedule { Frequency = Frequency.Weekly, StartDate = January1 };

        schedule.Begin(new DateOnly(2026, 1, 10));

        Assert.That(schedule.NextDueDate, Is.EqualTo(new DateOnly(2026, 1, 15)));
    }

    [Test]
    public void Beginning_on_a_schedule_date_makes_today_due()
    {
        var schedule = new RentSchedule { Frequency = Frequency.Weekly, StartDate = January1 };

        schedule.Begin(new DateOnly(2026, 1, 15));

        Assert.That(schedule.NextDueDate, Is.EqualTo(new DateOnly(2026, 1, 15)));
    }

    [Test]
    public void Beginning_a_schedule_that_has_already_ended_leaves_nothing_due()
    {
        var schedule = new RentSchedule { Frequency = Frequency.Monthly, StartDate = January1, EndDate = new DateOnly(2026, 1, 31) };

        schedule.Begin(new DateOnly(2026, 6, 1));

        Assert.That(schedule.NextDueDate, Is.Null);
    }

    [Test]
    public void Taking_due_dates_after_a_gap_returns_every_missed_date_and_moves_past_today()
    {
        var schedule = new RentSchedule { Frequency = Frequency.Weekly, StartDate = January1, NextDueDate = January1 };

        var due = schedule.TakeDueSlots(new DateOnly(2026, 1, 20));

        Assert.That(due.Select(slot => slot.Start), Is.EqualTo(new[]
        {
            January1, new DateOnly(2026, 1, 8), new DateOnly(2026, 1, 15),
        }));
        Assert.That(schedule.NextDueDate, Is.EqualTo(new DateOnly(2026, 1, 22)));
        Assert.That(schedule.LastCreatedDate, Is.EqualTo(new DateOnly(2026, 1, 15)));
        Assert.That(schedule.TakeDueSlots(new DateOnly(2026, 1, 20)), Is.Empty);
    }

    [Test]
    public void Taking_the_last_date_before_the_end_date_leaves_next_due_date_null()
    {
        var schedule = new RentSchedule
        {
            Frequency = Frequency.Weekly,
            StartDate = January1,
            EndDate = new DateOnly(2026, 1, 10),
            NextDueDate = January1,
        };

        var due = schedule.TakeDueSlots(new DateOnly(2026, 3, 1));

        Assert.That(due.Select(slot => slot.Start), Is.EqualTo(new[] { January1, new DateOnly(2026, 1, 8) }));
        Assert.That(schedule.NextDueDate, Is.Null);
    }

    [Test]
    public void A_schedule_that_has_created_nothing_yet_can_be_moved_to_start_earlier()
    {
        var schedule = new RentSchedule { Frequency = Frequency.Weekly, StartDate = new DateOnly(2026, 10, 20) };

        schedule.Reschedule(today: new DateOnly(2026, 10, 15));

        Assert.That(schedule.NextDueDate, Is.EqualTo(new DateOnly(2026, 10, 20)));
    }

    [Test]
    public void Rescheduling_never_lands_on_or_before_a_date_already_created()
    {
        // 13 October was created (and may since have been deleted); the edit moved the start back a week.
        var schedule = new RentSchedule
        {
            Frequency = Frequency.Weekly,
            StartDate = new DateOnly(2026, 10, 6),
            LastCreatedDate = new DateOnly(2026, 10, 13),
        };

        schedule.Reschedule(today: new DateOnly(2026, 10, 13));

        Assert.That(schedule.NextDueDate, Is.EqualTo(new DateOnly(2026, 10, 20)));
    }

    [Test]
    public void Rescheduling_never_goes_back_before_today()
    {
        var schedule = new RentSchedule
        {
            Frequency = Frequency.Weekly,
            StartDate = January1,
            LastCreatedDate = January1,
        };

        schedule.Reschedule(today: new DateOnly(2026, 3, 3));

        Assert.That(schedule.NextDueDate, Is.EqualTo(new DateOnly(2026, 3, 5)));
    }

    [Test]
    public void Rent_covers_its_period_copies_the_schedules_split_and_is_paid_by_the_primary()
    {
        var schedule = new RentSchedule
        {
            Id = 7,
            HouseholdId = 3,
            Frequency = Frequency.Monthly,
            StartDate = new DateOnly(2026, 2, 1),
            NextDueDate = new DateOnly(2026, 2, 1),
            Amount = 90m,
            SplitType = SplitType.Exact,
            Shares = [new Share(1, 20m), new Share(2, 30m), new Share(3, 40m)],
        };

        RentExpense rent = schedule.TakeDueOccurrences(new DateOnly(2026, 2, 10), primaryUserId: 2).Single();

        Assert.Multiple(() =>
        {
            Assert.That(rent.Tag, Is.EqualTo(ExpenseTag.Rent));
            Assert.That((rent.HouseholdId, rent.RentScheduleId, rent.Description), Is.EqualTo((3, (int?)7, "Rent")));
            Assert.That((rent.Date, rent.PeriodEnd), Is.EqualTo((new DateOnly(2026, 2, 1), (DateOnly?)new DateOnly(2026, 2, 28))));
            Assert.That((rent.Amount, rent.PaidByUserId, rent.SplitType), Is.EqualTo((90m, 2, SplitType.Exact)));
            Assert.That(rent.Shares.Select(share => (share.UserId, share.Amount)),
                Is.EqualTo(new[] { (1, 20m), (2, 30m), (3, 40m) }));
            // Copies, so a later edit of the schedule's split can't reach rent already created.
            Assert.That(rent.Shares[0], Is.Not.SameAs(schedule.Shares[0]));
        });
    }

    [Test]
    public void A_bill_starts_at_zero_split_equally_among_the_members_in_id_order()
    {
        var billType = new BillType
        {
            Id = 4,
            Name = "Internet",
            Frequency = Frequency.Monthly,
            StartDate = new DateOnly(2026, 2, 15),
            NextDueDate = new DateOnly(2026, 2, 15),
            Members = [new User { Id = 3 }, new User { Id = 1 }],
        };

        BillExpense bill = billType.TakeDueOccurrences(new DateOnly(2026, 2, 15), primaryUserId: 1).Single();

        Assert.Multiple(() =>
        {
            Assert.That((bill.Tag, bill.BillTypeId, bill.Description), Is.EqualTo((ExpenseTag.Bills, (int?)4, "Internet")));
            Assert.That((bill.Amount, bill.Date, bill.PaidByUserId), Is.EqualTo((0m, new DateOnly(2026, 2, 15), 1)));
            Assert.That(bill.SplitType, Is.EqualTo(SplitType.Equal));
            Assert.That(bill.Shares.Select(share => (share.UserId, share.Amount)), Is.EqualTo(new[] { (1, 0m), (3, 0m) }));
        });
    }
}
