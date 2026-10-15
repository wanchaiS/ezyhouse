using EzyHouse.Api.Core;
using EzyHouse.Api.Features.Households;
using EzyHouse.Domain.Balances;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;
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
        await ServiceFor(house.Bob).LeaveAsync(NoNextPrimary);
        await ServiceFor(house.Cara).LeaveAsync(NoNextPrimary);
        await ServiceFor(house.Alice).LeaveAsync(NoNextPrimary);
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
    public async Task Rent_that_fell_due_before_a_handover_stays_paid_by_the_old_primary()
    {
        // The hourly job hasn't created September's or October's rent yet.
        RentSchedule schedule = await AddMonthlyRentAsync(nextDueDate: new DateOnly(2026, 9, 1), ShareOf(house.Alice, 200m), ShareOf(house.Bob, 200m), ShareOf(house.Cara, 200m));

        await ServiceFor(house.Alice).TransferPrimaryAsync(new TransferPrimaryRequest(house.Bob.Id));

        await using EzyHouseDbContext check = testDb.NewContext();
        var rent = await check.Expenses.OfType<RentExpense>().OrderBy(expense => expense.Date).ToListAsync();
        Assert.That(rent.Select(expense => (expense.Date, expense.PaidByUserId)), Is.EqualTo(new[]
        {
            (new DateOnly(2026, 9, 1), house.Alice.Id),
            (new DateOnly(2026, 10, 1), house.Alice.Id),
        }));
        Assert.That((await check.RentSchedules.FindAsync(schedule.Id))!.NextDueDate, Is.EqualTo(new DateOnly(2026, 11, 1)));
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

        var refused = Assert.ThrowsAsync<BusinessRuleException>(() => ServiceFor(house.Bob).LeaveAsync(NoNextPrimary));

        Assert.That(refused!.Message, Is.EqualTo("You still owe money, or are owed money. Settle up first."));
    }

    [Test]
    public async Task Rent_already_due_that_the_job_has_not_created_yet_still_blocks_leaving()
    {
        await AddMonthlyRentAsync(nextDueDate: new DateOnly(2026, 10, 1), ShareOf(house.Alice, 300m), ShareOf(house.Bob, 300m));

        Assert.ThrowsAsync<BusinessRuleException>(() => ServiceFor(house.Bob).LeaveAsync(NoNextPrimary));
    }

    [Test]
    public async Task Being_in_a_running_rent_schedule_and_bill_type_does_not_block_leaving_when_there_are_no_debts()
    {
        await AddMonthlyRentAsync(nextDueDate: new DateOnly(2026, 11, 1), ShareOf(house.Alice, 300m), ShareOf(house.Bob, 300m));
        db.BillTypes.Add(InternetFor(house.Alice, house.Bob));
        await db.SaveChangesAsync();

        await ServiceFor(house.Bob).LeaveAsync(NoNextPrimary);

        await using EzyHouseDbContext check = testDb.NewContext();
        Assert.That((await check.Users.FindAsync(house.Bob.Id))!.HouseholdId, Is.Null);
    }

    [Test]
    public void The_primary_must_name_the_next_primary_while_others_remain()
    {
        Assert.ThrowsAsync<ValidationFailedException>(() => ServiceFor(house.Alice).LeaveAsync(NoNextPrimary));
        Assert.ThrowsAsync<ValidationFailedException>(() => ServiceFor(house.Alice).LeaveAsync(new LeaveInput(house.Alice.Id)));
    }

    [Test]
    public async Task The_next_primary_must_be_a_member_of_this_household()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<ValidationFailedException>(() => ServiceFor(house.Alice).LeaveAsync(new LeaveInput(other.Bob.Id)));
    }

    [Test]
    public async Task A_leaving_primary_hands_over_to_the_named_member()
    {
        await ServiceFor(house.Alice).LeaveAsync(new LeaveInput(house.Cara.Id));

        await using EzyHouseDbContext check = testDb.NewContext();
        Assert.That((await check.Households.FindAsync(house.Household.Id))!.PrimaryUserId, Is.EqualTo(house.Cara.Id));
        Assert.That((await check.Users.FindAsync(house.Alice.Id))!.HouseholdId, Is.Null);
    }

    [Test]
    public async Task A_primary_with_debts_stays_primary_and_in_the_household()
    {
        db.Expenses.Add(GroceriesPaidBy(house.Alice, house.Bob, 15m));
        await db.SaveChangesAsync();

        Assert.ThrowsAsync<BusinessRuleException>(() => ServiceFor(house.Alice).LeaveAsync(new LeaveInput(house.Bob.Id)));

        await using EzyHouseDbContext check = testDb.NewContext();
        Assert.That((await check.Households.FindAsync(house.Household.Id))!.PrimaryUserId, Is.EqualTo(house.Alice.Id));
        Assert.That((await check.Users.FindAsync(house.Alice.Id))!.HouseholdId, Is.EqualTo(house.Household.Id));
    }

    [Test]
    public async Task Leaving_takes_the_member_off_an_exact_rent_split_which_becomes_equal_among_the_rest()
    {
        RentSchedule schedule = await AddMonthlyRentAsync(
            nextDueDate: new DateOnly(2026, 11, 1),
            ShareOf(house.Alice, 500m), ShareOf(house.Bob, 300m), ShareOf(house.Cara, 100m));
        schedule.SplitType = SplitType.Exact;
        await db.SaveChangesAsync();

        await ServiceFor(house.Bob).LeaveAsync(NoNextPrimary);

        await using EzyHouseDbContext check = testDb.NewContext();
        RentSchedule saved = (await check.RentSchedules.FindAsync(schedule.Id))!;
        Assert.That(saved.SplitType, Is.EqualTo(SplitType.Equal));
        Assert.That(saved.Shares.Select(share => (share.UserId, share.Amount)),
            Is.EquivalentTo(new[] { (house.Alice.Id, 450m), (house.Cara.Id, 450m) }));
        Assert.That(saved.NextDueDate, Is.EqualTo(new DateOnly(2026, 11, 1)));
    }

    [Test]
    public async Task A_rent_schedule_or_bill_type_left_with_nobody_ends_today()
    {
        RentSchedule rent = await AddMonthlyRentAsync(nextDueDate: new DateOnly(2026, 11, 1), ShareOf(house.Bob, 600m));
        db.BillTypes.Add(InternetFor(house.Bob));
        await db.SaveChangesAsync();

        await ServiceFor(house.Bob).LeaveAsync(NoNextPrimary);

        await using EzyHouseDbContext check = testDb.NewContext();
        RentSchedule savedRent = (await check.RentSchedules.FindAsync(rent.Id))!;
        BillType savedInternet = await check.BillTypes.Include(billType => billType.Members).SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That((savedRent.EndDate, savedRent.NextDueDate, savedRent.Shares.Count), Is.EqualTo(((DateOnly?)Today, (DateOnly?)null, 0)));
            Assert.That((savedInternet.EndDate, savedInternet.NextDueDate, savedInternet.Members.Count), Is.EqualTo(((DateOnly?)Today, (DateOnly?)null, 0)));
        });
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
        // Cara still owes Alice for groceries, but that is between them and doesn't hold Bob back.
        db.Expenses.Add(GroceriesPaidBy(house.Alice, house.Cara, 30m));
        await db.SaveChangesAsync();

        await ServiceFor(house.Bob).LeaveAsync(NoNextPrimary);

        await using EzyHouseDbContext check = testDb.NewContext();
        Assert.That((await check.Users.FindAsync(house.Bob.Id))!.HouseholdId, Is.Null);
        List<BillType> billTypes = await check.BillTypes.Include(billType => billType.Members).ToListAsync();
        Assert.That(billTypes.SelectMany(billType => billType.Members).Select(member => member.Id),
            Does.Not.Contain(house.Bob.Id));
        Assert.That(billTypes.Single(billType => billType.Name == "Internet").Members, Has.Count.EqualTo(2));
    }

    private static readonly LeaveInput NoNextPrimary = new(null);

    private HouseholdService ServiceFor(User user) => new(
        new HouseholdRepository(db),
        TestUsers.SignIn(db, user),
        TestScheduleRunner.Create(db, Today),
        new HouseholdBalances(db, new BalanceCalculator()),
        new SplitCalculator([new EqualSplitStrategy(), new ExactSplitStrategy()]),
        new FixedClock(Today));

    private static Share ShareOf(User user, decimal amount) => new(user.Id, amount);

    private async Task<RentSchedule> AddMonthlyRentAsync(DateOnly nextDueDate, params Share[] shares)
    {
        var schedule = new RentSchedule
        {
            HouseholdId = house.Household.Id,
            Amount = shares.Sum(share => share.Amount),
            Frequency = Frequency.Monthly,
            StartDate = new DateOnly(2026, 9, 1),
            NextDueDate = nextDueDate,
            SplitType = SplitType.Equal,
            Shares = shares.ToList(),
        };
        db.RentSchedules.Add(schedule);
        await db.SaveChangesAsync();
        return schedule;
    }

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
        NextDueDate = new DateOnly(2026, 11, 15),
        Members = members.ToList(),
    };

    // Groceries the payer and one other person split equally, so the other person owes the payer their share.
    private GroceryExpense GroceriesPaidBy(User payer, User other, decimal eachShare) => new()
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
}
