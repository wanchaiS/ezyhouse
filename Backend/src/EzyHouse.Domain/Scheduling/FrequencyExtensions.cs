using EzyHouse.Domain.Enums;

namespace EzyHouse.Domain.Scheduling;

public static class FrequencyExtensions
{
    private static readonly Dictionary<Frequency, Func<DateOnly, int, DateOnly>> Steppers = new()
    {
        [Frequency.Weekly] = (date, steps) => date.AddDays(7 * steps),
        [Frequency.Fortnightly] = (date, steps) => date.AddDays(14 * steps),
        [Frequency.Monthly] = (date, steps) => date.AddMonths(steps),
        [Frequency.Quarterly] = (date, steps) => date.AddMonths(3 * steps),
    };

    public static DateOnly AddFrequency(this DateOnly start, Frequency frequency, int steps) =>
        Steppers[frequency](start, steps);
}
