using EzyHouse.Api.Features.Bills;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Tests.Features;

public class BillServiceTests
{
    private static readonly DateOnly Today = new(2026, 3, 15);

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
    public async Task A_bill_type_starting_today_creates_its_zero_dollar_bill_straight_away()
    {
        BillTypeDto created = await ServiceFor(house.Alice).CreateTypeAsync(MonthlyFrom(Today, house.Cara.Id, house.Bob.Id));

        BillExpense bill = (await BillsAsync()).Single();
        Assert.Multiple(() =>
        {
            Assert.That((bill.Description, bill.Amount, bill.Date, bill.BillTypeId), Is.EqualTo(("Electricity", 0m, Today, (int?)created.Id)));
            Assert.That((bill.PaidByUserId, bill.SplitType), Is.EqualTo((house.Alice.Id, SplitType.Equal)));
            Assert.That(bill.Shares.Select(share => (share.UserId, share.Amount)),
                Is.EquivalentTo(new[] { (house.Bob.Id, 0m), (house.Cara.Id, 0m) }));
            Assert.That(created.NextDueDate, Is.EqualTo(new DateOnly(2026, 4, 15)));
        });
    }

    [Test]
    public async Task A_bill_type_that_started_in_the_past_is_not_back_filled()
    {
        BillTypeDto created = await ServiceFor(house.Alice).CreateTypeAsync(MonthlyFrom(new DateOnly(2026, 1, 1), house.Alice.Id));

        Assert.That(await BillsAsync(), Is.Empty);
        Assert.That(created.NextDueDate, Is.EqualTo(new DateOnly(2026, 4, 1)));
    }

    [Test]
    public async Task A_bill_type_shared_by_nobody_is_rejected()
    {
        BillService alice = ServiceFor(house.Alice);
        BillTypeDto created = await alice.CreateTypeAsync(MonthlyFrom(Today, house.Alice.Id));

        var onCreate = Assert.ThrowsAsync<ValidationFailedException>(() => alice.CreateTypeAsync(MonthlyFrom(Today)));
        var onEdit = Assert.ThrowsAsync<ValidationFailedException>(() => alice.UpdateTypeAsync(created.Id, MonthlyFrom(Today)));

        Assert.That(onCreate!.Message, Is.EqualTo("Pick at least one person to share this bill."));
        Assert.That(onEdit!.Message, Is.EqualTo("Pick at least one person to share this bill."));
    }

    [Test]
    public async Task A_member_from_outside_the_household_is_rejected()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<ValidationFailedException>(() =>
            ServiceFor(house.Alice).CreateTypeAsync(MonthlyFrom(Today, house.Alice.Id, other.Bob.Id)));
    }

    [Test]
    public void A_housemate_who_is_not_primary_cannot_create_a_bill_type()
    {
        Assert.ThrowsAsync<ForbiddenException>(() =>
            ServiceFor(house.Bob).CreateTypeAsync(MonthlyFrom(Today, house.Bob.Id)));
    }

    [Test]
    public async Task Deleting_a_bill_type_keeps_its_bills_and_clears_the_link()
    {
        BillService alice = ServiceFor(house.Alice);
        BillTypeDto created = await alice.CreateTypeAsync(MonthlyFrom(Today, house.Alice.Id, house.Bob.Id));

        await alice.DeleteTypeAsync(created.Id);

        Assert.That((await BillsAsync()).Select(bill => (bill.Date, bill.BillTypeId)), Is.EqualTo(new[] { (Today, (int?)null) }));
        Assert.That(await alice.ListTypesAsync(), Is.Empty);
    }

    private BillService ServiceFor(User user) => new(
        new BillRepository(db),
        TestUsers.SignIn(db, user),
        TestScheduleRunner.Create(db, Today),
        new FixedClock(Today));

    private static BillTypeInput MonthlyFrom(DateOnly start, params int[] memberIds) =>
        new("Electricity", Frequency.Monthly, start, null, memberIds.ToList());

    private async Task<List<BillExpense>> BillsAsync()
    {
        await using EzyHouseDbContext check = testDb.NewContext();
        return await check.Expenses.OfType<BillExpense>().OrderBy(bill => bill.Date).ToListAsync();
    }
}
