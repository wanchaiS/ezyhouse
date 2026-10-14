using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Tests.Domain;

public class LifecycleTests
{
    private static readonly DateOnly Today = new(2026, 3, 1);

    [Test]
    public void Selling_an_item_that_is_already_sold_is_rejected()
    {
        var furniture = new Furniture();
        furniture.RecordSale(new FurnitureSale { Amount = 50m });

        Assert.That(furniture.Status, Is.EqualTo(FurnitureStatus.Sold));
        Assert.That(
            () => furniture.RecordSale(new FurnitureSale { Amount = 40m }),
            Throws.TypeOf<BusinessRuleException>());
    }

    [Test]
    public void Submitting_a_bill_twice_is_rejected()
    {
        var bill = new Bill();
        bill.Submit(100m, [new Share(1, 50m), new Share(2, 50m)], payeeUserId: 1, Today);

        Assert.That(
            () => bill.Submit(120m, [new Share(1, 60m), new Share(2, 60m)], payeeUserId: 1, Today),
            Throws.TypeOf<BusinessRuleException>());
    }

    [Test]
    public void Submitting_a_bill_ticks_only_the_payee()
    {
        var bill = new Bill();
        bill.Submit(100m, [new Share(1, 50m), new Share(2, 50m)], payeeUserId: 2, Today);

        Assert.That(bill.Status, Is.EqualTo(BillStatus.Submitted));
        Assert.That(bill.Shares.Select(share => (share.UserId, share.Paid, share.PaidDate)), Is.EqualTo(new (int, bool, DateOnly?)[]
        {
            (1, false, null),
            (2, true, Today),
        }));
    }

    [Test]
    public void Ticking_a_share_on_a_pending_bill_is_rejected()
    {
        var bill = new Bill();

        Assert.That(
            () => bill.SetSharePaid(1, true, Today),
            Throws.TypeOf<BusinessRuleException>());
    }

    [Test]
    public void Unticking_a_share_clears_its_paid_date()
    {
        var share = new PaidShare(1, 30m, true, Today);

        share.SetPaid(false, Today);

        Assert.That(share.Paid, Is.False);
        Assert.That(share.PaidDate, Is.Null);
    }

    [Test]
    public void Bond_contributions_over_the_total_are_rejected()
    {
        var bond = new Bond();

        Assert.That(
            () => bond.Update(100m, Today, [new BondContribution { Amount = 60m }, new BondContribution { Amount = 50m }]),
            Throws.TypeOf<ValidationFailedException>());
    }
}
