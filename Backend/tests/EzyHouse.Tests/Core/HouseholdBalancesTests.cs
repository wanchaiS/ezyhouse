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
    public async Task Rent_bills_groceries_furniture_and_settlements_all_count_when_read_from_the_database()
    {
        await using TestDb testDb = await TestDb.CreateAsync();
        await using EzyHouseDbContext db = testDb.NewContext();
        TestHousehold house = await TestHousehold.CreateAsync(db);
        int alice = house.Alice.Id, bob = house.Bob.Id, cara = house.Cara.Id;
        int householdId = house.Household.Id;

        T Paid<T>(T expense, int payer, params Share[] shares) where T : Expense
        {
            expense.HouseholdId = householdId;
            expense.Description = expense.Tag.ToString();
            expense.Amount = shares.Sum(share => share.Amount);
            expense.Date = new DateOnly(2026, 10, 1);
            expense.PaidByUserId = payer;
            expense.CreatedByUserId = payer;
            expense.SplitType = SplitType.Exact;
            expense.Shares = shares.ToList();
            return expense;
        }

        db.Expenses.AddRange(
            Paid(new RentExpense(), alice, new Share(alice, 100m), new Share(bob, 100m), new Share(cara, 100m)),
            Paid(new BillExpense(), alice, new Share(alice, 30m), new Share(bob, 30m), new Share(cara, 30m)),
            Paid(new FurnitureExpense(), bob, new Share(alice, 20m), new Share(bob, 20m), new Share(cara, 20m)),
            Paid(new GroceryExpense(), cara, new Share(bob, 15m), new Share(cara, 15m)));
        db.Settlements.Add(new Settlement
        {
            HouseholdId = householdId, FromUserId = bob, ToUserId = alice, Amount = 50m,
            Date = new DateOnly(2026, 10, 2), CreatedByUserId = bob,
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var (nets, debts) = await new HouseholdBalances(db, new BalanceCalculator())
            .CalculateAsync(householdId, house.MemberIds);

        // Bob → Alice: 100 rent + 30 bill − 20 furniture − 50 settled. Cara → Alice: 100 + 30.
        // Cara → Bob: 20 furniture − 15 groceries.
        Assert.That(debts, Is.EquivalentTo(new[]
        {
            new Debt(bob, alice, 60m),
            new Debt(cara, alice, 130m),
            new Debt(cara, bob, 5m),
        }));
        Assert.That((nets[alice], nets[bob], nets[cara]), Is.EqualTo((190m, -55m, -135m)));
    }
}
