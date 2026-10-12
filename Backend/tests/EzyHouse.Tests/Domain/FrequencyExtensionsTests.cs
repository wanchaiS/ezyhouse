using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Scheduling;

namespace EzyHouse.Tests.Domain;

public class FrequencyExtensionsTests
{
    private static readonly DateOnly Start = new(2026, 1, 15);

    [TestCase(Frequency.Weekly, "2026-01-22")]
    [TestCase(Frequency.Fortnightly, "2026-01-29")]
    [TestCase(Frequency.Monthly, "2026-02-15")]
    [TestCase(Frequency.Quarterly, "2026-04-15")]
    public void Adds_one_step_of_each_frequency(Frequency frequency, string expected)
    {
        DateOnly actual = Start.AddFrequency(frequency, 1);

        Assert.That(actual, Is.EqualTo(DateOnly.Parse(expected)));
    }
}
