using EzyHouse.Api.Core;
using EzyHouse.Domain.Balances;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;
using EzyHouse.Infrastructure.Data;
using EzyHouse.Tests.Support;

namespace EzyHouse.Tests.Core;

public class HouseholdBalancesTests
{
    [Test]
    public async Task The_couch_bought_and_sold_gives_the_API_md_debts_when_read_from_the_database()
    {
        await using TestDb testDb = await TestDb.CreateAsync();
        await using EzyHouseDbContext db = testDb.NewContext();
        TestHousehold house = await TestHousehold.CreateAsync(db);
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

        var (nets, debts) = await new HouseholdBalances(db, new BalanceCalculator())
            .CalculateAsync(house.Household.Id, house.MemberIds);

        Assert.That(debts, Is.EquivalentTo(new[]
        {
            new Debt(bob, alice, 200m),
            new Debt(cara, alice, 300m),
            new Debt(cara, bob, 100m),
        }));
        Assert.That(nets[alice], Is.EqualTo(500m));
        Assert.That(nets[bob], Is.EqualTo(-100m));
        Assert.That(nets[cara], Is.EqualTo(-400m));
    }
}
