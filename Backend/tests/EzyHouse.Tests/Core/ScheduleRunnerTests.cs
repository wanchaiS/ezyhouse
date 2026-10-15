using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Tests.Core;

public class ScheduleRunnerTests
{
    private static readonly DateOnly September1 = new(2026, 9, 1);
    private static readonly DateOnly Today = new(2026, 10, 13);

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
    public async Task After_the_API_was_off_every_missed_date_is_created_with_its_own_date()
    {
        RentSchedule schedule = await AddWeeklyRentAsync(house, nextDueDate: September1);

        await TestScheduleRunner.Create(db, Today).RunDueForEveryHouseholdAsync();

        Assert.That(await RentDatesAsync(), Is.EqualTo(new[]
        {
            September1, new DateOnly(2026, 9, 8), new DateOnly(2026, 9, 15), new DateOnly(2026, 9, 22),
            new DateOnly(2026, 9, 29), new DateOnly(2026, 10, 6), Today,
        }));
        await using EzyHouseDbContext check = testDb.NewContext();
        RentSchedule saved = (await check.RentSchedules.FindAsync(schedule.Id))!;
        Assert.That((saved.NextDueDate, saved.LastCreatedDate), Is.EqualTo(((DateOnly?)new DateOnly(2026, 10, 20), (DateOnly?)Today)));
    }

    [Test]
    public async Task Running_again_the_same_day_creates_nothing_more()
    {
        await AddWeeklyRentAsync(house, nextDueDate: September1);

        await TestScheduleRunner.Create(db, Today).RunDueForEveryHouseholdAsync();
        await TestScheduleRunner.Create(db, Today).RunDueForEveryHouseholdAsync();

        Assert.That(await RentDatesAsync(), Has.Count.EqualTo(7));
    }

    [Test]
    public async Task A_deleted_rent_expense_is_never_created_again()
    {
        await AddWeeklyRentAsync(house, nextDueDate: new DateOnly(2026, 10, 6));
        await TestScheduleRunner.Create(db, Today).RunDueForEveryHouseholdAsync();
        RentExpense october6 = await db.Expenses.OfType<RentExpense>().SingleAsync(rent => rent.Date == new DateOnly(2026, 10, 6));
        db.Expenses.Remove(october6);
        await db.SaveChangesAsync();

        await TestScheduleRunner.Create(db, Today).RunDueForEveryHouseholdAsync();
        await TestScheduleRunner.Create(db, Today.AddDays(7)).RunDueForEveryHouseholdAsync();

        Assert.That(await RentDatesAsync(), Is.EqualTo(new[] { Today, Today.AddDays(7) }));
    }

    [Test]
    public async Task Each_expense_is_paid_by_whoever_is_primary_when_it_is_created()
    {
        await AddWeeklyRentAsync(house, nextDueDate: new DateOnly(2026, 10, 6));
        await TestScheduleRunner.Create(db, new DateOnly(2026, 10, 6)).RunDueForEveryHouseholdAsync();

        house.Household.PrimaryUserId = house.Bob.Id;
        await db.SaveChangesAsync();
        await TestScheduleRunner.Create(db, Today).RunDueForEveryHouseholdAsync();

        await using EzyHouseDbContext check = testDb.NewContext();
        var payers = await check.Expenses.OfType<RentExpense>()
            .OrderBy(rent => rent.Date)
            .Select(rent => new { rent.Date, rent.PaidByUserId })
            .ToListAsync();
        Assert.That(payers.Select(rent => (rent.Date, rent.PaidByUserId)), Is.EqualTo(new[]
        {
            (new DateOnly(2026, 10, 6), house.Alice.Id),
            (Today, house.Bob.Id),
        }));
    }

    [Test]
    public async Task A_bill_type_with_nobody_in_it_is_skipped_and_the_other_schedules_still_run()
    {
        db.BillTypes.Add(new BillType
        {
            HouseholdId = house.Household.Id,
            Name = "Gas",
            Frequency = Frequency.Monthly,
            StartDate = September1,
            NextDueDate = September1,
        });
        db.BillTypes.Add(new BillType
        {
            HouseholdId = house.Household.Id,
            Name = "Water",
            Frequency = Frequency.Quarterly,
            StartDate = September1,
            NextDueDate = September1,
            Members = [house.Bob],
        });
        await db.SaveChangesAsync();
        await AddWeeklyRentAsync(house, nextDueDate: Today);

        await TestScheduleRunner.Create(db, Today).RunDueForEveryHouseholdAsync();

        await using EzyHouseDbContext check = testDb.NewContext();
        Assert.That(await check.Expenses.Select(expense => expense.Description).ToListAsync(),
            Is.EquivalentTo(new[] { "Rent", "Water" }));
    }

    [Test]
    public async Task Running_for_one_household_leaves_other_households_alone()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");
        await AddWeeklyRentAsync(house, nextDueDate: Today);
        await AddWeeklyRentAsync(other, nextDueDate: Today);

        await TestScheduleRunner.Create(db, Today).RunDueForHouseholdAsync(house.Household.Id);

        await using EzyHouseDbContext check = testDb.NewContext();
        Assert.That(await check.Expenses.Select(expense => expense.HouseholdId).ToListAsync(),
            Is.EqualTo(new[] { house.Household.Id }));
    }

    private async Task<RentSchedule> AddWeeklyRentAsync(TestHousehold home, DateOnly nextDueDate)
    {
        var schedule = new RentSchedule
        {
            HouseholdId = home.Household.Id,
            Amount = 750m,
            Frequency = Frequency.Weekly,
            StartDate = September1,
            NextDueDate = nextDueDate,
            SplitType = SplitType.Equal,
            Shares = [new Share(home.Alice.Id, 250m), new Share(home.Bob.Id, 250m), new Share(home.Cara.Id, 250m)],
        };
        db.RentSchedules.Add(schedule);
        await db.SaveChangesAsync();
        return schedule;
    }

    private async Task<List<DateOnly>> RentDatesAsync()
    {
        await using EzyHouseDbContext check = testDb.NewContext();
        return await check.Expenses.OfType<RentExpense>().Select(rent => rent.Date).OrderBy(date => date).ToListAsync();
    }
}
