using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Tests.Domain;

public class ExactSplitStrategyTests
{
    private readonly ExactSplitStrategy strategy = new();

    [Test]
    public void Rejects_shares_that_add_up_to_less_than_the_total()
    {
        var shares = new ShareInput[] { new(1, 40m), new(2, 40m) };

        Assert.That(
            () => strategy.Split(100m, shares),
            Throws.TypeOf<ValidationFailedException>());
    }

    [Test]
    public void Rejects_a_share_that_is_missing_its_amount()
    {
        var shares = new ShareInput[] { new(1, 50m), new(2, null) };

        Assert.That(
            () => strategy.Split(100m, shares),
            Throws.TypeOf<ValidationFailedException>());
    }

    [Test]
    public void Rejects_a_negative_share()
    {
        var shares = new ShareInput[] { new(1, 110m), new(2, -10m) };

        Assert.That(
            () => strategy.Split(100m, shares),
            Throws.TypeOf<ValidationFailedException>());
    }

    [Test]
    public void Rejects_a_share_with_more_than_2_decimals()
    {
        var shares = new ShareInput[] { new(1, 50.005m), new(2, 49.995m) };

        Assert.That(
            () => strategy.Split(100m, shares),
            Throws.TypeOf<ValidationFailedException>());
    }

    [Test]
    public void Allows_zero_shares_for_a_free_disposal()
    {
        var shares = new ShareInput[] { new(1, 0m), new(2, 0m) };

        Assert.That(strategy.Split(0m, shares).Select(share => share.Amount), Is.EqualTo(new[] { 0m, 0m }));
    }
}
