using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Exceptions;

namespace EzyHouse.Tests.Domain;

public class BondTests
{
    [Test]
    public void Bond_contributions_over_the_total_are_rejected()
    {
        var bond = new Bond();

        Assert.That(
            () => bond.Update(100m, new DateOnly(2026, 3, 1), [new BondContribution { Amount = 60m }, new BondContribution { Amount = 50m }]),
            Throws.TypeOf<ValidationFailedException>());
    }
}
