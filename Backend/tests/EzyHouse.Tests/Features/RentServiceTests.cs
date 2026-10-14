using EzyHouse.Api.Core;
using EzyHouse.Api.Features.Rent;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;

namespace EzyHouse.Tests.Features;

public class RentServiceTests
{
    private static readonly DateOnly January1 = new(2026, 1, 1);

    private TestDb testDb = null!;
    private EzyHouseDbContext db = null!;
    private TestHousehold house = null!;

    [SetUp]
    public async Task SetUp()
    {
        testDb = await TestDb.CreateAsync();
        db = testDb.NewContext();
        house = await TestHousehold.CreateAsync(db);
    }

    [TearDown]
    public async Task TearDown()
    {
        await db.DisposeAsync();
        await testDb.DisposeAsync();
    }

    [Test]
    public async Task Reading_periods_twice_on_the_same_day_creates_no_duplicates()
    {
        RentService alice = Service(house.Alice, new DateOnly(2026, 3, 15));
        await alice.CreateScheduleAsync(MonthlyInput(January1));

        await alice.ListPeriodsAsync();
        IReadOnlyList<RentPeriodDto> periods = await alice.ListPeriodsAsync();

        Assert.That(periods.Select(period => period.StartDate), Is.EqualTo(new[]
        {
            new DateOnly(2026, 3, 1), new DateOnly(2026, 2, 1), January1,
        }));
    }

    [Test]
    public async Task After_a_primary_handover_new_periods_go_to_the_new_primary_and_old_ones_keep_their_payee()
    {
        RentService before = Service(house.Alice, new DateOnly(2026, 2, 10));
        await before.CreateScheduleAsync(MonthlyInput(January1));
        await before.ListPeriodsAsync();

        house.Household.PrimaryUserId = house.Bob.Id;
        await db.SaveChangesAsync();

        IReadOnlyList<RentPeriodDto> periods = await Service(house.Bob, new DateOnly(2026, 3, 15)).ListPeriodsAsync();

        Assert.That(periods.Select(period => period.Payee.Id), Is.EqualTo(new[]
        {
            house.Bob.Id, house.Alice.Id, house.Alice.Id,
        }));
    }

    [Test]
    public async Task Moving_the_start_date_earlier_does_not_recreate_past_periods()
    {
        RentService alice = Service(house.Alice, new DateOnly(2026, 3, 15));
        RentScheduleDto schedule = await alice.CreateScheduleAsync(MonthlyInput(new DateOnly(2026, 3, 1)));
        await alice.ListPeriodsAsync();

        await alice.UpdateScheduleAsync(schedule.Id, MonthlyInput(January1));
        IReadOnlyList<RentPeriodDto> periods = await alice.ListPeriodsAsync();

        Assert.That(periods.Select(period => period.StartDate), Is.EqualTo(new[] { new DateOnly(2026, 3, 1) }));
    }

    [Test]
    public async Task Unticking_a_share_clears_its_paid_date()
    {
        RentService bob = Service(house.Bob, new DateOnly(2026, 1, 15));
        await Service(house.Alice, new DateOnly(2026, 1, 15)).CreateScheduleAsync(MonthlyInput(January1));
        RentPeriodDto period = (await bob.ListPeriodsAsync()).Single();

        RentPeriodDto ticked = await bob.SetSharePaidAsync(period.Id, house.Bob.Id, true);
        RentPeriodDto unticked = await bob.SetSharePaidAsync(period.Id, house.Bob.Id, false);

        PaidShareDto tickedShare = ticked.Shares.Single(share => share.User.Id == house.Bob.Id);
        PaidShareDto untickedShare = unticked.Shares.Single(share => share.User.Id == house.Bob.Id);
        Assert.That(tickedShare.PaidDate, Is.EqualTo(new DateOnly(2026, 1, 15)));
        Assert.That(untickedShare.Paid, Is.False);
        Assert.That(untickedShare.PaidDate, Is.Null);
    }

    [Test]
    public async Task Someone_else_cannot_tick_a_housemates_share_unless_they_are_primary()
    {
        DateOnly today = new(2026, 1, 15);
        await Service(house.Alice, today).CreateScheduleAsync(MonthlyInput(January1));
        RentPeriodDto period = (await Service(house.Alice, today).ListPeriodsAsync()).Single();

        Assert.ThrowsAsync<ForbiddenException>(() =>
            Service(house.Cara, today).SetSharePaidAsync(period.Id, house.Bob.Id, true));
        RentPeriodDto byPrimary = await Service(house.Alice, today).SetSharePaidAsync(period.Id, house.Bob.Id, true);
        Assert.That(byPrimary.Shares.Single(share => share.User.Id == house.Bob.Id).Paid, Is.True);
    }

    [Test]
    public async Task A_period_from_another_household_is_not_found()
    {
        DateOnly today = new(2026, 1, 15);
        await Service(house.Alice, today).CreateScheduleAsync(MonthlyInput(January1));
        RentPeriodDto period = (await Service(house.Alice, today).ListPeriodsAsync()).Single();
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<NotFoundException>(() =>
            Service(other.Alice, today).SetSharePaidAsync(period.Id, other.Alice.Id, true));
    }

    [Test]
    public void A_housemate_who_is_not_primary_cannot_create_a_schedule()
    {
        Assert.ThrowsAsync<ForbiddenException>(() =>
            Service(house.Bob, January1).CreateScheduleAsync(MonthlyInput(January1)));
    }

    [Test]
    public void An_end_date_before_the_start_date_is_rejected()
    {
        RentScheduleInput input = MonthlyInput(January1) with { EndDate = January1.AddDays(-1) };

        Assert.ThrowsAsync<ValidationFailedException>(() => Service(house.Alice, January1).CreateScheduleAsync(input));
    }

    [Test]
    public async Task Deleting_a_schedule_removes_its_periods()
    {
        RentService alice = Service(house.Alice, new DateOnly(2026, 2, 15));
        RentScheduleDto schedule = await alice.CreateScheduleAsync(MonthlyInput(January1));
        await alice.ListPeriodsAsync();

        await alice.DeleteScheduleAsync(schedule.Id);

        await using EzyHouseDbContext check = testDb.NewContext();
        Assert.That(check.RentPeriods.Count(), Is.Zero);
    }

    private RentService Service(User user, DateOnly today) => new(
        new RentRepository(db),
        TestUsers.SignIn(db, user),
        new RecurringChargeGenerator(db),
        new SplitCalculator([new EqualSplitStrategy(), new ExactSplitStrategy()]),
        new FixedClock(today));

    private RentScheduleInput MonthlyInput(DateOnly start) => new(
        900m,
        Frequency.Monthly,
        start,
        null,
        SplitType.Equal,
        [new ShareInput(house.Alice.Id, null), new ShareInput(house.Bob.Id, null), new ShareInput(house.Cara.Id, null)]);
}
