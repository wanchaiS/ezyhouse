using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Tests.Core;

public class RecurringChargeGeneratorTests
{
    private static readonly DateOnly Today = new(2026, 10, 13);

    [Test]
    public async Task Reading_rent_twice_on_the_same_day_creates_each_period_once()
    {
        await using TestDb testDb = await TestDb.CreateAsync();
        await using EzyHouseDbContext db = testDb.NewContext();
        TestHousehold house = await TestHousehold.CreateAsync(db);
        db.RentSchedules.Add(new RentSchedule
        {
            HouseholdId = house.Household.Id,
            Amount = 750m,
            Frequency = Frequency.Weekly,
            StartDate = new DateOnly(2026, 9, 1),
            SplitType = SplitType.Equal,
            Shares = [new Share(house.Alice.Id, 250m), new Share(house.Bob.Id, 250m), new Share(house.Cara.Id, 250m)],
        });
        await db.SaveChangesAsync();
        var generator = new RecurringChargeGenerator(db);

        await generator.GenerateRentPeriodsAsync(house.Household, Today);
        await generator.GenerateRentPeriodsAsync(house.Household, Today);

        List<DateOnly> starts = await db.RentPeriods.Select(period => period.StartDate).OrderBy(start => start).ToListAsync();
        Assert.That(starts, Is.EqualTo(new[]
        {
            new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 8), new DateOnly(2026, 9, 15), new DateOnly(2026, 9, 22),
            new DateOnly(2026, 9, 29), new DateOnly(2026, 10, 6), new DateOnly(2026, 10, 13),
        }));
    }

    [Test]
    public async Task Reading_bills_twice_on_the_same_day_creates_each_placeholder_once()
    {
        await using TestDb testDb = await TestDb.CreateAsync();
        await using EzyHouseDbContext db = testDb.NewContext();
        TestHousehold house = await TestHousehold.CreateAsync(db);
        db.BillTypes.Add(new BillType
        {
            HouseholdId = house.Household.Id,
            Name = "Internet",
            Frequency = Frequency.Monthly,
            StartDate = new DateOnly(2026, 8, 15),
            Members = [house.Alice, house.Bob, house.Cara],
        });
        await db.SaveChangesAsync();
        var generator = new RecurringChargeGenerator(db);

        await generator.GenerateBillPlaceholdersAsync(house.Household, Today);
        await generator.GenerateBillPlaceholdersAsync(house.Household, Today);

        List<Bill> bills = await db.Bills.OrderBy(bill => bill.DueDate).ToListAsync();
        Assert.That(bills.Select(bill => bill.DueDate), Is.EqualTo(new[] { new DateOnly(2026, 8, 15), new DateOnly(2026, 9, 15) }));
        Assert.That(bills.Select(bill => bill.Status), Is.All.EqualTo(BillStatus.Pending));
    }
}
