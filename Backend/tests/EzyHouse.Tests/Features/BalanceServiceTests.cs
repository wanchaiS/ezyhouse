using EzyHouse.Api.Core;
using EzyHouse.Api.Features.Balances;
using EzyHouse.Domain.Balances;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
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
    public async Task The_couch_bought_by_alice_and_sold_by_cara_gives_the_API_md_debts()
    {
        await AddCouchAsync();

        BalancesDto result = await ServiceFor(house.Alice).GetBalancesAsync();

        Assert.That(Debts(result), Is.EquivalentTo(new[]
        {
            (house.Bob.Id, house.Alice.Id, 200m),
            (house.Cara.Id, house.Alice.Id, 300m),
            (house.Cara.Id, house.Bob.Id, 100m),
        }));
        Assert.That(result.Members.Select(member => member.User.Id), Is.EquivalentTo(house.MemberIds));
    }

    [Test]
    public async Task A_settlement_from_bob_to_alice_of_200_clears_that_debt()
    {
        await AddCouchAsync();

        SettlementDto settlement = await ServiceFor(house.Bob).CreateSettlementAsync(
            new SettlementInput(house.Bob.Id, house.Alice.Id, 200m));
        BalancesDto result = await ServiceFor(house.Alice).GetBalancesAsync();

        Assert.That(settlement.Date, Is.EqualTo(Today));
        Assert.That(settlement.CreatedBy.Id, Is.EqualTo(house.Bob.Id));
        Assert.That(Debts(result), Is.EquivalentTo(new[]
        {
            (house.Cara.Id, house.Alice.Id, 300m),
            (house.Cara.Id, house.Bob.Id, 100m),
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
        int alice = house.Alice.Id, bob = house.Bob.Id, cara = house.Cara.Id;
        var couch = new Furniture
        {
            HouseholdId = house.Household.Id,
            Name = "Couch",
            CreatedByUserId = alice,
            Transactions =
            [
                new FurniturePurchase
                {
                    Date = new DateOnly(2026, 6, 1), Amount = 600m, CounterpartyUserId = alice, SplitType = SplitType.Equal,
                    Shares = [new Share(alice, 200m), new Share(bob, 200m), new Share(cara, 200m)],
                },
            ],
        };
        couch.RecordSale(new FurnitureSale
        {
            Date = new DateOnly(2026, 10, 1), Amount = 300m, CounterpartyUserId = cara, SplitType = SplitType.Equal,
            Shares = [new Share(alice, 100m), new Share(bob, 100m), new Share(cara, 100m)],
        });
        db.Furniture.Add(couch);
        await db.SaveChangesAsync();
    }
}
