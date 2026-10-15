using EzyHouse.Domain.Balances;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Tests.Domain;

public class BalanceCalculatorTests
{
    private readonly BalanceCalculator calculator = new();
    private static readonly int[] Members = [1, 2, 3];

    [Test]
    public void Expenses_of_every_tag_count_the_same_way()
    {
        var rent = new RentExpense { Amount = 300m, PaidByUserId = 1, Shares = [new Share(1, 100m), new Share(2, 100m), new Share(3, 100m)] };
        var furniture = new FurnitureExpense { Amount = 60m, PaidByUserId = 2, Shares = [new Share(1, 30m), new Share(2, 30m)] };

        var (nets, debts) = calculator.Calculate([rent, furniture], Members);

        Assert.That(debts, Is.EquivalentTo(new Debt[] { new(2, 1, 70m), new(3, 1, 100m) }));
        Assert.That(nets.Values.Sum(), Is.EqualTo(0m));
    }

    [Test]
    public void A_settlement_of_any_amount_pays_down_the_balance_between_that_pair_only()
    {
        var groceries = new GroceryExpense { Amount = 90m, PaidByUserId = 1, Shares = [new Share(1, 30m), new Share(2, 30m), new Share(3, 30m)] };
        var settlement = new Settlement { FromUserId = 2, ToUserId = 1, Amount = 12.5m };

        var (_, debts) = calculator.Calculate([groceries, settlement], Members);

        Assert.That(debts, Is.EquivalentTo(new Debt[] { new(2, 1, 17.5m), new(3, 1, 30m) }));
    }

    [Test]
    public void Settlement_cancels_the_payer_debt_and_leaves_the_other_share()
    {
        var expense = new GroceryExpense
        {
            Amount = 90m,
            PaidByUserId = 1,
            Shares = [new Share(1, 30m), new Share(2, 30m), new Share(3, 30m)],
        };
        var settlement = new Settlement { FromUserId = 2, ToUserId = 1, Amount = 30m };

        var (nets, debts) = calculator.Calculate([expense, settlement], Members);

        Assert.That(debts, Is.EqualTo(new Debt[] { new(3, 1, 30m) }));
        Assert.That(nets[2], Is.EqualTo(0m));
    }

    [Test]
    public void Cancelling_debts_keep_both_pairwise_entries_when_the_net_is_zero()
    {
        IBalanceContributor youOweAlice = new LedgerContributor(new LedgerEntry(2, 1, 10m));
        IBalanceContributor bobOwesYou = new LedgerContributor(new LedgerEntry(3, 2, 10m));

        var (nets, debts) = calculator.Calculate([youOweAlice, bobOwesYou], Members);

        Assert.That(nets[2], Is.EqualTo(0m));
        Assert.That(debts, Is.EquivalentTo(new Debt[]
        {
            new(2, 1, 10m),
            new(3, 2, 10m),
        }));
    }

    private sealed class LedgerContributor(LedgerEntry entry) : IBalanceContributor
    {
        public IEnumerable<LedgerEntry> GetLedgerEntries() => [entry];
    }
}
