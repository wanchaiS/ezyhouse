using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Tests.Domain;

public class EqualSplitStrategyTests
{
    private readonly EqualSplitStrategy strategy = new();

    [Test]
    public void Splits_one_hundred_three_ways_and_gives_the_leftover_cent_to_the_first_share()
    {
        IReadOnlyList<Share> shares = strategy.Split(100m,
        [
            new ShareInput(1, null),
            new ShareInput(2, null),
            new ShareInput(3, null),
        ]);

        Assert.That(shares.Select(share => share.Amount), Is.EqualTo(new[] { 33.34m, 33.33m, 33.33m }));
        Assert.That(shares.Sum(share => share.Amount), Is.EqualTo(100m));
    }

    [Test]
    public void Splits_sixty_three_ways_evenly()
    {
        IReadOnlyList<Share> shares = strategy.Split(60m,
        [
            new ShareInput(1, null),
            new ShareInput(2, null),
            new ShareInput(3, null),
        ]);

        Assert.That(shares.Select(share => share.Amount), Is.EqualTo(new[] { 20m, 20m, 20m }));
    }
}
