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
    public void Couch_sale_makes_the_receiver_owe_the_other_share_holders()
    {
        var sale = new FurnitureSale
        {
            Amount = 300m,
            CounterpartyUserId = 3,
            Shares = [new Share(1, 100m), new Share(2, 100m), new Share(3, 100m)],
        };

        var (_, debts) = calculator.Calculate([sale], Members);

        Assert.That(debts, Is.EquivalentTo(new Debt[]
        {
            new(3, 1, 100m),
            new(3, 2, 100m),
        }));
    }

    [Test]
    public void Couch_purchase_and_sale_match_the_worked_example()
    {
        var purchase = new FurniturePurchase
        {
            Amount = 600m,
            CounterpartyUserId = 1,
            Shares = [new Share(1, 200m), new Share(2, 200m), new Share(3, 200m)],
        };
        var sale = new FurnitureSale
        {
            Amount = 300m,
            CounterpartyUserId = 3,
            Shares = [new Share(1, 100m), new Share(2, 100m), new Share(3, 100m)],
        };

        var (nets, debts) = calculator.Calculate([purchase, sale], Members);

        Assert.That(debts, Is.EquivalentTo(new Debt[]
        {
            new(2, 1, 200m),
            new(3, 1, 300m),
            new(3, 2, 100m),
        }));
        Assert.That(nets[1], Is.EqualTo(500m));
        Assert.That(nets[2], Is.EqualTo(-100m));
        Assert.That(nets[3], Is.EqualTo(-400m));
        Assert.That(nets.Values.Sum(), Is.EqualTo(0m));
    }

    [Test]
    public void Settlement_cancels_the_payer_debt_and_leaves_the_other_share()
    {
        var expense = new Expense
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
