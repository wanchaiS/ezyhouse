using EzyHouse.Api.Core;
using EzyHouse.Api.Features.Bills;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;
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

    private BillService ServiceFor(User user) => new(
        new BillRepository(db),
        TestUsers.SignIn(db, user),
        new RecurringChargeGenerator(db),
        new SplitCalculator([new EqualSplitStrategy(), new ExactSplitStrategy()]),
        new FixedClock(Today));

    private async Task<BillTypeDto> CreateMonthlyTypeAsync(params int[] memberIds) =>
        await ServiceFor(house.Alice).CreateTypeAsync(new BillTypeInput(
            "Electricity", Frequency.Monthly, new DateOnly(2026, 2, 1), null, memberIds.ToList()));

    private async Task<IReadOnlyList<BillDto>> PendingBillsAsync() =>
        await ServiceFor(house.Alice).ListAsync(BillStatus.Pending);

    private static SubmitBillInput Equal(decimal amount) => new(amount, SplitType.Equal, null);

    [Test]
    public async Task Listing_bills_creates_placeholders_newest_first()
    {
        await CreateMonthlyTypeAsync(house.Alice.Id, house.Bob.Id);

        IReadOnlyList<BillDto> bills = await ServiceFor(house.Bob).ListAsync(null);

        Assert.That(bills.Select(bill => bill.DueDate),
            Is.EqualTo(new[] { new DateOnly(2026, 3, 1), new DateOnly(2026, 2, 1) }));
        Assert.That(bills.All(bill => bill.Status == BillStatus.Pending && bill.Shares.Count == 0));
    }

    [Test]
    public async Task Equal_submit_splits_across_members_by_user_id_and_ticks_the_payee()
    {
        await CreateMonthlyTypeAsync(house.Cara.Id, house.Bob.Id, house.Alice.Id);
        BillDto bill = (await PendingBillsAsync())[0];

        BillDto submitted = await ServiceFor(house.Alice).SubmitAsync(bill.Id, Equal(100m));

        int[] ids = new[] { house.Alice.Id, house.Bob.Id, house.Cara.Id }.Order().ToArray();
        Assert.That(submitted.Shares.Select(share => share.User.Id), Is.EqualTo(ids));
        Assert.That(submitted.Shares.Select(share => share.Amount), Is.EqualTo(new[] { 33.34m, 33.33m, 33.33m }));
        Assert.That(submitted.Status, Is.EqualTo(BillStatus.Submitted));
        Assert.That(submitted.Payee!.Id, Is.EqualTo(house.Alice.Id));
        Assert.That(submitted.Shares.Single(share => share.User.Id == house.Alice.Id).Paid, Is.True);
        Assert.That(submitted.Shares.Where(share => share.User.Id != house.Alice.Id).All(share => !share.Paid));
    }

    [Test]
    public async Task Submitting_a_bill_twice_is_a_409()
    {
        await CreateMonthlyTypeAsync(house.Alice.Id, house.Bob.Id);
        BillDto bill = (await PendingBillsAsync())[0];
        await ServiceFor(house.Alice).SubmitAsync(bill.Id, Equal(90m));

        Assert.ThrowsAsync<BusinessRuleException>(() => ServiceFor(house.Alice).SubmitAsync(bill.Id, Equal(50m)));
    }

    [Test]
    public async Task An_exact_submit_must_add_up()
    {
        await CreateMonthlyTypeAsync(house.Alice.Id, house.Bob.Id);
        BillDto bill = (await PendingBillsAsync())[0];

        Assert.ThrowsAsync<ValidationFailedException>(() => ServiceFor(house.Alice).SubmitAsync(bill.Id,
            new SubmitBillInput(100m, SplitType.Exact, [new ShareInput(house.Alice.Id, 60m), new ShareInput(house.Bob.Id, 30m)])));
        Assert.ThrowsAsync<ValidationFailedException>(() => ServiceFor(house.Alice).SubmitAsync(bill.Id,
            new SubmitBillInput(100m, SplitType.Exact, null)));
    }

    [Test]
    public async Task Only_the_primary_can_submit()
    {
        await CreateMonthlyTypeAsync(house.Alice.Id, house.Bob.Id);
        BillDto bill = (await PendingBillsAsync())[0];

        Assert.ThrowsAsync<ForbiddenException>(() => ServiceFor(house.Bob).SubmitAsync(bill.Id, Equal(10m)));
    }

    [Test]
    public async Task Ticking_a_share_on_a_pending_bill_is_a_409()
    {
        await CreateMonthlyTypeAsync(house.Alice.Id, house.Bob.Id);
        BillDto bill = (await PendingBillsAsync())[0];

        Assert.ThrowsAsync<BusinessRuleException>(() => ServiceFor(house.Bob).SetSharePaidAsync(bill.Id, house.Bob.Id, true));
    }

    [Test]
    public async Task A_member_can_tick_their_own_share_but_not_someone_elses()
    {
        await CreateMonthlyTypeAsync(house.Alice.Id, house.Bob.Id, house.Cara.Id);
        BillDto bill = (await PendingBillsAsync())[0];
        await ServiceFor(house.Alice).SubmitAsync(bill.Id, Equal(30m));

        BillDto ticked = await ServiceFor(house.Bob).SetSharePaidAsync(bill.Id, house.Bob.Id, true);

        Assert.That(ticked.Shares.Single(share => share.User.Id == house.Bob.Id).PaidDate, Is.EqualTo(Today));
        Assert.ThrowsAsync<ForbiddenException>(() => ServiceFor(house.Bob).SetSharePaidAsync(bill.Id, house.Cara.Id, true));
    }

    [Test]
    public async Task Ticking_someone_not_in_the_split_is_a_404()
    {
        await CreateMonthlyTypeAsync(house.Alice.Id, house.Bob.Id);
        BillDto bill = (await PendingBillsAsync())[0];
        await ServiceFor(house.Alice).SubmitAsync(bill.Id, Equal(30m));

        Assert.ThrowsAsync<NotFoundException>(() => ServiceFor(house.Alice).SetSharePaidAsync(bill.Id, house.Cara.Id, true));
    }

    [Test]
    public async Task A_bill_type_member_must_be_a_current_household_member()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<ValidationFailedException>(() => CreateMonthlyTypeAsync(house.Alice.Id, other.Bob.Id));
    }

    [Test]
    public async Task Another_households_bill_is_a_404()
    {
        await CreateMonthlyTypeAsync(house.Alice.Id);
        BillDto bill = (await PendingBillsAsync())[0];
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<NotFoundException>(() => ServiceFor(other.Alice).SubmitAsync(bill.Id, Equal(10m)));
    }

    [Test]
    public async Task Deleting_a_type_removes_pending_bills_and_keeps_submitted_ones()
    {
        BillTypeDto type = await CreateMonthlyTypeAsync(house.Alice.Id, house.Bob.Id);
        IReadOnlyList<BillDto> pending = await PendingBillsAsync();
        BillDto older = pending.Single(bill => bill.DueDate == new DateOnly(2026, 2, 1));
        await ServiceFor(house.Alice).SubmitAsync(older.Id, Equal(80m));

        await ServiceFor(house.Alice).DeleteTypeAsync(type.Id);

        await using EzyHouseDbContext check = testDb.NewContext();
        List<Bill> remaining = await check.Bills.ToListAsync();
        Assert.That(remaining, Has.Count.EqualTo(1));
        Assert.That(remaining[0].Id, Is.EqualTo(older.Id));
        Assert.That(remaining[0].BillTypeId, Is.Null);
        Assert.That(remaining[0].BillTypeName, Is.EqualTo("Electricity"));
        Assert.That(await check.BillTypes.AnyAsync(), Is.False);
    }

    [Test]
    public async Task A_bill_whose_type_was_deleted_can_only_be_submitted_exactly()
    {
        BillTypeDto type = await CreateMonthlyTypeAsync(house.Alice.Id, house.Bob.Id);
        Bill orphan = new() { HouseholdId = house.Household.Id, BillTypeName = "Gas", DueDate = Today };
        db.Bills.Add(orphan);
        await db.SaveChangesAsync();
        await ServiceFor(house.Alice).DeleteTypeAsync(type.Id);

        Assert.ThrowsAsync<ValidationFailedException>(() => ServiceFor(house.Alice).SubmitAsync(orphan.Id, Equal(10m)));
        BillDto submitted = await ServiceFor(house.Alice).SubmitAsync(orphan.Id,
            new SubmitBillInput(10m, SplitType.Exact, [new ShareInput(house.Bob.Id, 10m)]));
        Assert.That(submitted.BillType.Id, Is.Null);
    }
}
