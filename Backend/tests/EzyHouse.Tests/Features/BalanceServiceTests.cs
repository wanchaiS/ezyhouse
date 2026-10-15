using EzyHouse.Api.Core;
using EzyHouse.Api.Features.Balances;
using EzyHouse.Api.Features.Expenses;
using EzyHouse.Domain.Balances;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;

namespace EzyHouse.Tests.Features;

public class BalanceServiceTests
{
    private static readonly DateOnly Today = new(2026, 10, 15);

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
    public async Task Furniture_counts_in_balances_and_every_member_is_listed()
    {
        await AddCouchAsync();

        BalancesDto result = await ServiceFor(house.Alice).GetBalancesAsync();

        Assert.That(Debts(result), Is.EquivalentTo(new[]
        {
            (house.Bob.Id, house.Alice.Id, 200m),
            (house.Cara.Id, house.Alice.Id, 200m),
        }));
        Assert.That(result.Members.Select(member => member.User.Id), Is.EquivalentTo(house.MemberIds));
    }

    [Test]
    public async Task A_settlement_of_any_amount_pays_down_only_that_pairs_balance()
    {
        await AddCouchAsync();

        SettlementDto settlement = await ServiceFor(house.Bob).CreateSettlementAsync(
            new SettlementInput(house.Bob.Id, house.Alice.Id, 72.35m));
        BalancesDto result = await ServiceFor(house.Alice).GetBalancesAsync();

        Assert.That(settlement.Date, Is.EqualTo(Today));
        Assert.That(settlement.CreatedBy.Id, Is.EqualTo(house.Bob.Id));
        Assert.That(Debts(result), Is.EquivalentTo(new[]
        {
            (house.Bob.Id, house.Alice.Id, 127.65m),
            (house.Cara.Id, house.Alice.Id, 200m),
        }));
    }

    [Test]
    public async Task A_bill_at_zero_counts_for_nothing_until_the_primary_enters_its_amount()
    {
        var bill = new BillExpense
        {
            HouseholdId = house.Household.Id,
            Description = "Internet",
            Amount = 0m,
            Date = new DateOnly(2026, 10, 1),
            PaidByUserId = house.Alice.Id,
            CreatedByUserId = house.Alice.Id,
            SplitType = SplitType.Equal,
            Shares = [new Share(house.Alice.Id, 0m), new Share(house.Bob.Id, 0m), new Share(house.Cara.Id, 0m)],
        };
        db.Expenses.Add(bill);
        await db.SaveChangesAsync();
        BalancesDto before = await ServiceFor(house.Alice).GetBalancesAsync();

        await new ExpenseService(new ExpenseRepository(db), TestUsers.SignIn(db, house.Alice),
                new SplitCalculator([new EqualSplitStrategy(), new ExactSplitStrategy()]))
            .UpdateAsync(bill.Id, new ExpenseInput("Internet", 90m, bill.Date, house.Alice.Id, SplitType.Equal,
                [new ShareInput(house.Alice.Id, null), new ShareInput(house.Bob.Id, null), new ShareInput(house.Cara.Id, null)]));
        BalancesDto after = await ServiceFor(house.Alice).GetBalancesAsync();

        Assert.That(before.Debts, Is.Empty);
        Assert.That(Debts(after), Is.EquivalentTo(new[]
        {
            (house.Bob.Id, house.Alice.Id, 30m),
            (house.Cara.Id, house.Alice.Id, 30m),
        }));
    }

    [Test]
    public void Someone_who_is_neither_payer_nor_payee_cannot_record_a_settlement()
    {
        Assert.ThrowsAsync<ForbiddenException>(() => ServiceFor(house.Cara).CreateSettlementAsync(
            new SettlementInput(house.Bob.Id, house.Alice.Id, 50m)));
    }

    [Test]
    public void A_settlement_to_yourself_is_rejected()
    {
        Assert.ThrowsAsync<ValidationFailedException>(() => ServiceFor(house.Bob).CreateSettlementAsync(
            new SettlementInput(house.Bob.Id, house.Bob.Id, 50m)));
    }

    [Test]
    public async Task A_settlement_with_someone_outside_the_household_is_rejected()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");

        Assert.ThrowsAsync<ValidationFailedException>(() => ServiceFor(house.Bob).CreateSettlementAsync(
            new SettlementInput(house.Bob.Id, other.Alice.Id, 50m)));
    }

    [Test]
    public async Task Settlements_are_listed_newest_first_and_only_for_the_callers_household()
    {
        TestHousehold other = await TestHousehold.CreateAsync(db, "OTHRHSE3");
        db.Settlements.AddRange(
            new Settlement { HouseholdId = house.Household.Id, FromUserId = house.Bob.Id, ToUserId = house.Alice.Id, Amount = 10m, Date = new DateOnly(2026, 9, 1), CreatedByUserId = house.Bob.Id },
            new Settlement { HouseholdId = house.Household.Id, FromUserId = house.Cara.Id, ToUserId = house.Alice.Id, Amount = 20m, Date = new DateOnly(2026, 10, 1), CreatedByUserId = house.Cara.Id },
            new Settlement { HouseholdId = other.Household.Id, FromUserId = other.Bob.Id, ToUserId = other.Alice.Id, Amount = 30m, Date = new DateOnly(2026, 10, 2), CreatedByUserId = other.Bob.Id });
        await db.SaveChangesAsync();

        IReadOnlyList<SettlementDto> list = await ServiceFor(house.Alice).ListSettlementsAsync();

        Assert.That(list.Select(item => item.Amount), Is.EqualTo(new[] { 20m, 10m }));
    }

    [Test]
    public async Task A_debt_with_someone_who_has_left_still_shows_their_name()
    {
        await AddCouchAsync();
        house.Cara.HouseholdId = null;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        BalancesDto result = await ServiceFor(house.Alice).GetBalancesAsync();

        Assert.That(result.Debts.Where(debt => debt.From.Id == house.Cara.Id).Select(debt => debt.From.Name),
            Has.Some.EqualTo("Cara"));
    }

    private BalanceService ServiceFor(User user) => new(
        TestUsers.SignIn(db, user),
        new HouseholdBalances(db, new BalanceCalculator()),
        new SettlementRepository(db),
        new FixedClock(Today));

    private static IEnumerable<(int, int, decimal)> Debts(BalancesDto result) =>
        result.Debts.Select(debt => (debt.From.Id, debt.To.Id, debt.Amount));

    private async Task AddCouchAsync()
    {
        db.Expenses.Add(new FurnitureExpense
        {
            HouseholdId = house.Household.Id,
            Description = "Couch",
            Amount = 600m,
            Date = new DateOnly(2026, 6, 1),
            PaidByUserId = house.Alice.Id,
            CreatedByUserId = house.Alice.Id,
            SplitType = SplitType.Equal,
            Shares = [new Share(house.Alice.Id, 200m), new Share(house.Bob.Id, 200m), new Share(house.Cara.Id, 200m)],
        });
        await db.SaveChangesAsync();
    }
}
