using EzyHouse.Api.Features.Rent;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Tests.Features;

public class RentServiceTests
{
    private static readonly DateOnly March15 = new(2026, 3, 15);

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
    public async Task Creating_a_schedule_never_back_fills_and_creates_todays_rent_straight_away()
    {
        RentScheduleDto schedule = await Service(house.Alice, March15).CreateScheduleAsync(MonthlyInput(new DateOnly(2026, 1, 15)));

        List<RentExpense> rent = await RentAsync();
        Assert.That(rent.Select(expense => (expense.Date, expense.PeriodEnd, expense.RentScheduleId, expense.Amount, expense.PaidByUserId)),
            Is.EqualTo(new[] { (March15, (DateOnly?)new DateOnly(2026, 4, 14), (int?)schedule.Id, 900m, house.Alice.Id) }));
        Assert.That(schedule.NextDueDate, Is.EqualTo(new DateOnly(2026, 4, 15)));
    }

    [Test]
    public async Task A_schedule_starting_later_creates_nothing_until_its_first_date()
    {
        RentScheduleDto schedule = await Service(house.Alice, March15).CreateScheduleAsync(MonthlyInput(new DateOnly(2026, 3, 20)));

        Assert.That(await RentAsync(), Is.Empty);
        Assert.That(schedule.NextDueDate, Is.EqualTo(new DateOnly(2026, 3, 20)));
    }

    [Test]
    public async Task A_schedule_not_started_yet_can_be_moved_earlier_but_never_before_today()
    {
        RentService alice = Service(house.Alice, March15);
        RentScheduleDto created = await alice.CreateScheduleAsync(MonthlyInput(new DateOnly(2026, 4, 1)));

        RentScheduleDto earlier = await alice.UpdateScheduleAsync(created.Id, MonthlyInput(new DateOnly(2026, 3, 20)));
        RentScheduleDto inThePast = await alice.UpdateScheduleAsync(created.Id, MonthlyInput(new DateOnly(2026, 1, 10)));

        Assert.That(earlier.NextDueDate, Is.EqualTo(new DateOnly(2026, 3, 20)));
        Assert.That(inThePast.NextDueDate, Is.EqualTo(new DateOnly(2026, 4, 10)));
        Assert.That(await RentAsync(), Is.Empty);
    }

    [Test]
    public async Task Editing_never_brings_back_a_date_already_created_even_after_its_rent_was_deleted()
    {
        RentService alice = Service(house.Alice, March15);
        RentScheduleDto schedule = await alice.CreateScheduleAsync(MonthlyInput(March15));
        db.Expenses.Remove(await db.Expenses.OfType<RentExpense>().SingleAsync());
        await db.SaveChangesAsync();

        RentScheduleDto edited = await alice.UpdateScheduleAsync(schedule.Id, MonthlyInput(new DateOnly(2026, 2, 15)) with { Amount = 1200m });

        Assert.That(edited.NextDueDate, Is.EqualTo(new DateOnly(2026, 4, 15)));
        Assert.That(await RentAsync(), Is.Empty);
    }

    [Test]
    public async Task Editing_changes_only_rent_not_yet_created()
    {
        RentService alice = Service(house.Alice, March15);
        RentScheduleDto schedule = await alice.CreateScheduleAsync(MonthlyInput(March15));

        await alice.UpdateScheduleAsync(schedule.Id, MonthlyInput(March15) with { Amount = 1200m });

        Assert.That((await RentAsync()).Select(expense => expense.Amount), Is.EqualTo(new[] { 900m }));
    }

    [Test]
    public async Task Rent_due_before_an_edit_is_created_under_the_old_settings()
    {
        RentService alice = Service(house.Alice, new DateOnly(2026, 3, 1));
        RentScheduleDto schedule = await alice.CreateScheduleAsync(MonthlyInput(new DateOnly(2026, 3, 1)));

        // The hourly job hasn't run since 1 April yet when the amount is changed.
        await Service(house.Alice, new DateOnly(2026, 4, 1)).UpdateScheduleAsync(schedule.Id, MonthlyInput(new DateOnly(2026, 3, 1)) with { Amount = 1200m });

        Assert.That((await RentAsync()).Select(expense => (expense.Date, expense.Amount)), Is.EqualTo(new[]
        {
            (new DateOnly(2026, 3, 1), 900m),
            (new DateOnly(2026, 4, 1), 900m),
        }));
    }

    [Test]
    public async Task Ending_the_schedule_before_its_next_date_leaves_nothing_due()
    {
        RentService alice = Service(house.Alice, March15);
        RentScheduleDto schedule = await alice.CreateScheduleAsync(MonthlyInput(March15));

        RentScheduleDto ended = await alice.UpdateScheduleAsync(schedule.Id, MonthlyInput(March15) with { EndDate = new DateOnly(2026, 4, 1) });

        Assert.That(ended.NextDueDate, Is.Null);
    }

    [Test]
    public async Task Deleting_a_schedule_keeps_its_rent_and_clears_the_link()
    {
        RentService alice = Service(house.Alice, March15);
        RentScheduleDto schedule = await alice.CreateScheduleAsync(MonthlyInput(March15));

        await alice.DeleteScheduleAsync(schedule.Id);

        List<RentExpense> rent = await RentAsync();
        Assert.That(rent.Select(expense => (expense.Date, expense.RentScheduleId)), Is.EqualTo(new[] { (March15, (int?)null) }));
    }

    [Test]
    public void A_housemate_who_is_not_primary_cannot_create_a_schedule()
    {
        Assert.ThrowsAsync<ForbiddenException>(() =>
            Service(house.Bob, March15).CreateScheduleAsync(MonthlyInput(March15)));
    }

    [Test]
    public void An_end_date_before_the_start_date_is_rejected()
    {
        RentScheduleInput input = MonthlyInput(March15) with { EndDate = March15.AddDays(-1) };

        Assert.ThrowsAsync<ValidationFailedException>(() => Service(house.Alice, March15).CreateScheduleAsync(input));
    }

    [Test]
    public async Task A_schedule_shared_by_nobody_is_rejected()
    {
        RentService alice = Service(house.Alice, March15);
        RentScheduleDto schedule = await alice.CreateScheduleAsync(MonthlyInput(new DateOnly(2026, 4, 1)));
        RentScheduleInput nobody = MonthlyInput(new DateOnly(2026, 4, 1)) with { Shares = [] };

        var onCreate = Assert.ThrowsAsync<ValidationFailedException>(() => alice.CreateScheduleAsync(nobody));
        var onEdit = Assert.ThrowsAsync<ValidationFailedException>(() => alice.UpdateScheduleAsync(schedule.Id, nobody));

        Assert.That(onCreate!.Message, Is.EqualTo("Pick at least one person to share the rent."));
        Assert.That(onEdit!.Message, Is.EqualTo("Pick at least one person to share the rent."));
    }

    private RentService Service(User user, DateOnly today) => new(
        new RentRepository(db),
        TestUsers.SignIn(db, user),
        TestScheduleRunner.Create(db, today),
        new SplitCalculator([new EqualSplitStrategy(), new ExactSplitStrategy()]),
        new FixedClock(today));

    private async Task<List<RentExpense>> RentAsync()
    {
        await using EzyHouseDbContext check = testDb.NewContext();
        return await check.Expenses.OfType<RentExpense>().OrderBy(expense => expense.Date).ToListAsync();
    }

    private RentScheduleInput MonthlyInput(DateOnly start) => new(
        900m,
        Frequency.Monthly,
        start,
        null,
        SplitType.Equal,
        [new ShareInput(house.Alice.Id, null), new ShareInput(house.Bob.Id, null), new ShareInput(house.Cara.Id, null)]);
}
