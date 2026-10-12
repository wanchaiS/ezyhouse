using EzyHouse.Domain.Extensions;

namespace EzyHouse.Tests.Domain;

public class MoneyExtensionsTests
{
    [Test]
    public void Accepts_an_amount_with_two_decimal_places()
    {
        Assert.That(10.50m.HasAtMostTwoDecimals(), Is.True);
    }

    [Test]
    public void Rejects_an_amount_with_three_decimal_places()
    {
        Assert.That(10.505m.HasAtMostTwoDecimals(), Is.False);
    }
}
