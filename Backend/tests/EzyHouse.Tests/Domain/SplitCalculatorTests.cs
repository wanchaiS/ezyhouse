using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Tests.Domain;

public class SplitCalculatorTests
{
    private readonly SplitCalculator calculator = new([new EqualSplitStrategy(), new ExactSplitStrategy()]);
    private readonly IReadOnlySet<int> members = new HashSet<int> { 1, 2, 3 };

    [Test]
    public void Rejects_an_empty_split()
    {
        Assert.That(
            () => calculator.Split(10m, SplitType.Equal, [], members),
            Throws.TypeOf<ValidationFailedException>());
    }

    [Test]
    public void Rejects_a_person_named_twice()
    {
        var shares = new ShareInput[] { new(1, null), new(1, null) };

        Assert.That(
            () => calculator.Split(10m, SplitType.Equal, shares, members),
            Throws.TypeOf<ValidationFailedException>());
    }

    [Test]
    public void Rejects_a_person_who_is_not_a_member()
    {
        var shares = new ShareInput[] { new(1, null), new(9, null) };

        Assert.That(
            () => calculator.Split(10m, SplitType.Equal, shares, members),
            Throws.TypeOf<ValidationFailedException>());
    }
}
