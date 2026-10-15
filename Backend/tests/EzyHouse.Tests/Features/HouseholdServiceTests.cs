using EzyHouse.Api.Core;
using EzyHouse.Api.Features.Households;
using EzyHouse.Domain.Balances;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.ValueObjects;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Tests.Features;

public class HouseholdServiceTests
{
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
    public async Task Creating_a_household_makes_the_caller_its_primary_and_only_member()
    {
        User dave = await AddUserWithoutHouseholdAsync("Dave");

        HouseholdDto created = await ServiceFor(dave).CreateAsync(new CreateHouseholdRequest("  Dave's Flat "));

        Assert.That(created.Name, Is.EqualTo("Dave's Flat"));
        Assert.That(created.Primary, Is.EqualTo(new UserRefDto(dave.Id, "Dave")));
        Assert.That(created.Members, Is.EqualTo(new[] { new UserRefDto(dave.Id, "Dave") }));
        Assert.That(created.InviteCode, Does.Match("^[A-HJ-NP-Z2-9]{8}$"));
        await using EzyHouseDbContext check = testDb.NewContext();
        Assert.That((await check.Users.FindAsync(dave.Id))!.HouseholdId, Is.EqualTo(created.Id));
    }

    [Test]
    public void Creating_a_household_while_already_in_one_is_a_409()
    {
        Assert.ThrowsAsync<BusinessRuleException>(() =>
            ServiceFor(house.Bob).CreateAsync(new CreateHouseholdRequest("Bob's Other Place")));
    }

    [Test]
    public async Task Getting_the_household_when_not_in_one_is_a_404()
    {
        User dave = await AddUserWithoutHouseholdAsync("Dave");

        Assert.ThrowsAsync<NotFoundException>(() => ServiceFor(dave).GetAsync());
    }

    [Test]
    public async Task Joining_adds_the_caller_as_a_member_but_to_no_bill_type()
    {
        db.BillTypes.Add(InternetFor(house.Alice, house.Bob, house.Cara));
        await db.SaveChangesAsync();
        User dave = await AddUserWithoutHouseholdAsync("Dave");

        HouseholdDto joined = await ServiceFor(dave).JoinAsync(new JoinHouseholdRequest("testhse2"));

        Assert.That(joined.Id, Is.EqualTo(house.Household.Id));
        Assert.That(joined.Members.Select(member => member.Id), Does.Contain(dave.Id));
        await using EzyHouseDbContext check = testDb.NewContext();
        BillType internet = await check.BillTypes.Include(billType => billType.Members).SingleAsync();
        Assert.That(internet.Members.Select(member => member.Id), Does.Not.Contain(dave.Id));
    }

    [Test]
    public async Task Joining_with_an_unknown_code_is_a_404()
    {
        User dave = await AddUserWithoutHouseholdAsync("Dave");

        Assert.ThrowsAsync<NotFoundException>(() =>
            ServiceFor(dave).JoinAsync(new JoinHouseholdRequest("ZZZZZZZZ")));
    }

    [Test]
    public async Task Joining_while_already_in_a_household_is_a_409()
    {
        await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<BusinessRuleException>(() =>
            ServiceFor(house.Bob).JoinAsync(new JoinHouseholdRequest("OTHRHSE3")));
    }

    [Test]
    public async Task The_last_member_can_leave_and_the_archived_household_then_cannot_be_joined()
    {
        await ServiceFor(house.Bob).LeaveAsync();
        await ServiceFor(house.Cara).LeaveAsync();
        await ServiceFor(house.Alice).LeaveAsync();
        User dave = await AddUserWithoutHouseholdAsync("Dave");

        Assert.ThrowsAsync<NotFoundException>(() =>
            ServiceFor(dave).JoinAsync(new JoinHouseholdRequest("TESTHSE2")));
        await using EzyHouseDbContext check = testDb.NewContext();
        Assert.That(await check.Households.AnyAsync(household => household.Id == house.Household.Id), Is.True);
    }

    [Test]
    public async Task Handing_over_primary_makes_the_chosen_member_primary()
    {
        HouseholdDto updated = await ServiceFor(house.Alice).TransferPrimaryAsync(new TransferPrimaryRequest(house.Bob.Id));

        Assert.That(updated.Primary.Id, Is.EqualTo(house.Bob.Id));
        await using EzyHouseDbContext check = testDb.NewContext();
        Assert.That((await check.Households.FindAsync(house.Household.Id))!.PrimaryUserId, Is.EqualTo(house.Bob.Id));
    }

    [Test]
    public async Task Rent_that_fell_due_before_a_handover_stays_owed_to_the_old_primary()
    {
        // Nobody has opened the Rent tab, so September's and October's periods don't exist yet.
        db.RentSchedules.Add(new RentSchedule
        {
            HouseholdId = house.Household.Id,
            Amount = 600m,
            Frequency = Frequency.Monthly,
            StartDate = new DateOnly(2026, 9, 1),
            SplitType = SplitType.Equal,
            Shares = [new Share(house.Alice.Id, 200m), new Share(house.Bob.Id, 200m), new Share(house.Cara.Id, 200m)],
        });
        await db.SaveChangesAsync();

        await ServiceFor(house.Alice).TransferPrimaryAsync(new TransferPrimaryRequest(house.Bob.Id));

        await using EzyHouseDbContext check = testDb.NewContext();
        List<RentPeriod> periods = await check.RentPeriods.OrderBy(period => period.StartDate).ToListAsync();
        Assert.That(periods.Select(period => (period.StartDate, period.PayeeUserId)), Is.EqualTo(new[]
        {
            (new DateOnly(2026, 9, 1), house.Alice.Id),
            (new DateOnly(2026, 10, 1), house.Alice.Id),
        }));
    }

    [Test]
    public void Only_the_primary_can_hand_over_primary()
    {
        Assert.ThrowsAsync<ForbiddenException>(() =>
            ServiceFor(house.Bob).TransferPrimaryAsync(new TransferPrimaryRequest(house.Cara.Id)));
    }

    [Test]
    public async Task Handing_over_primary_to_someone_outside_the_household_is_a_400()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<ValidationFailedException>(() =>
            ServiceFor(house.Alice).TransferPrimaryAsync(new TransferPrimaryRequest(other.Bob.Id)));
    }

    [Test]
    public async Task Debts_that_cancel_out_to_a_zero_net_still_block_leaving()
    {
        // Bob owes Alice 10 and Cara owes Bob 10: Bob's net is zero, but both debts remain.
        db.Expenses.AddRange(
            GroceriesPaidBy(house.Alice, house.Bob, 10m),
            GroceriesPaidBy(house.Bob, house.Cara, 10m));
        await db.SaveChangesAsync();

        var refused = Assert.ThrowsAsync<BusinessRuleException>(() => ServiceFor(house.Bob).LeaveAsync());

        Assert.That(refused!.Message, Is.EqualTo(
            "You still owe money, or are owed money, for groceries or furniture. Settle up first."));
    }

    [Test]
    public void The_primary_cannot_leave_while_others_remain()
    {
        var refused = Assert.ThrowsAsync<BusinessRuleException>(() => ServiceFor(house.Alice).LeaveAsync());

        Assert.That(refused!.Message, Is.EqualTo(
            "You're the primary and others are still in the household. Hand over primary first."));
    }

    [Test]
    public async Task Rent_that_fell_due_before_anyone_opened_the_rent_tab_still_blocks_leaving()
    {
        // The schedule has ended, so only its never-generated period can block Bob.
        db.RentSchedules.Add(new RentSchedule
        {
            HouseholdId = house.Household.Id,
            Amount = 600m,
            Frequency = Frequency.Monthly,
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 30),
            SplitType = SplitType.Equal,
            Shares = [new Share(house.Alice.Id, 200m), new Share(house.Bob.Id, 200m), new Share(house.Cara.Id, 200m)],
        });
        await db.SaveChangesAsync();

        var refused = Assert.ThrowsAsync<BusinessRuleException>(() => ServiceFor(house.Bob).LeaveAsync());

        Assert.That(refused!.Message, Is.EqualTo("A rent share owed by you or to you is unpaid."));
    }

    [Test]
    public async Task Every_problem_is_listed_in_one_409()
    {
        // Alice is primary, is owed groceries, is in a running rent schedule whose shares are owed to
        // her, and paid a bill that Bob and Cara haven't repaid.
        db.Expenses.Add(GroceriesPaidBy(house.Alice, house.Bob, 15m));
        db.RentSchedules.Add(new RentSchedule
        {
            HouseholdId = house.Household.Id,
            Amount = 750m,
            Frequency = Frequency.Weekly,
            StartDate = new DateOnly(2026, 10, 6),
            SplitType = SplitType.Equal,
            Shares = [new Share(house.Alice.Id, 250m), new Share(house.Bob.Id, 250m), new Share(house.Cara.Id, 250m)],
        });
        db.Bills.Add(SubmittedPowerBillPaidBy(house.Alice));
        await db.SaveChangesAsync();

        var refused = Assert.ThrowsAsync<BusinessRuleException>(() => ServiceFor(house.Alice).LeaveAsync());

        Assert.That(refused!.Message, Is.EqualTo(string.Join("\n",
            "You still owe money, or are owed money, for groceries or furniture. Settle up first.",
            "A rent share owed by you or to you is unpaid.",
            "You're in the split of a rent schedule that hasn't ended. Ask the primary to end it or take you off it.",
            "A bill share owed by you or to you is unpaid.",
            "You're the primary and others are still in the household. Hand over primary first.")));
    }

    [Test]
    public async Task A_clean_leave_clears_the_household_and_removes_the_member_from_every_bill_type()
    {
        db.BillTypes.AddRange(
            InternetFor(house.Alice, house.Bob, house.Cara),
            new BillType
            {
                HouseholdId = house.Household.Id,
                Name = "Water",
                Frequency = Frequency.Quarterly,
                StartDate = new DateOnly(2026, 7, 1),
                Members = [house.Bob, house.Cara],
            });
        // Cara still owes Alice for power, but that is between them and doesn't hold Bob back.
        Bill power = SubmittedPowerBillPaidBy(house.Alice);
        power.SetSharePaid(house.Bob.Id, true, Today);
        db.Bills.Add(power);
        await db.SaveChangesAsync();

        await ServiceFor(house.Bob).LeaveAsync();

        await using EzyHouseDbContext check = testDb.NewContext();
        Assert.That((await check.Users.FindAsync(house.Bob.Id))!.HouseholdId, Is.Null);
        List<BillType> billTypes = await check.BillTypes.Include(billType => billType.Members).ToListAsync();
        Assert.That(billTypes.SelectMany(billType => billType.Members).Select(member => member.Id),
            Does.Not.Contain(house.Bob.Id));
        Assert.That(billTypes.Single(billType => billType.Name == "Internet").Members, Has.Count.EqualTo(2));
    }

    private HouseholdService ServiceFor(User user) => new(
        new HouseholdRepository(db, new HouseholdBalances(db, new BalanceCalculator())),
        TestUsers.SignIn(db, user),
        new RecurringChargeGenerator(db),
        new FixedClock(Today));

    private async Task<User> AddUserWithoutHouseholdAsync(string name)
    {
        var user = new User { Name = name, Email = $"{name.ToLowerInvariant()}@example.com" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private BillType InternetFor(params User[] members) => new()
    {
        HouseholdId = house.Household.Id,
        Name = "Internet",
        Frequency = Frequency.Monthly,
        StartDate = new DateOnly(2026, 8, 15),
        Members = members.ToList(),
    };

    // An expense the payer and one other person split equally, so the other person owes the payer their share.
    private Expense GroceriesPaidBy(User payer, User other, decimal eachShare) => new()
    {
        HouseholdId = house.Household.Id,
        Description = "Groceries",
        Amount = eachShare * 2,
        Date = new DateOnly(2026, 10, 1),
        PaidByUserId = payer.Id,
        CreatedByUserId = payer.Id,
        SplitType = SplitType.Equal,
        Shares = [new Share(payer.Id, eachShare), new Share(other.Id, eachShare)],
    };

    private Bill SubmittedPowerBillPaidBy(User payee)
    {
        var bill = new Bill { HouseholdId = house.Household.Id, BillTypeName = "Power", DueDate = new DateOnly(2026, 10, 1) };
        bill.Submit(
            90m,
            [new Share(house.Alice.Id, 30m), new Share(house.Bob.Id, 30m), new Share(house.Cara.Id, 30m)],
            payee.Id,
            Today);
        return bill;
    }
}
