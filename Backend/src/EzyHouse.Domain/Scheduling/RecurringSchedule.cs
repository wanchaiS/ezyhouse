using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Domain.Scheduling;

public abstract class RecurringSchedule<TOccurrence> : IHouseholdOwned
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }

    public Household? Household { get; set; }

    public Frequency Frequency { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Yields each slot from <see cref="StartDate"/> until <paramref name="upTo"/> or <see cref="EndDate"/>,
    /// whichever comes first. Dates are always calculated from the start plus a step count.
    /// </summary>
    public IEnumerable<ScheduleSlot> GetSlots(DateOnly upTo)
    {
        DateOnly last = EndDate is { } end && end < upTo ? end : upTo;
        for (int step = 0; ; step++)
        {
            DateOnly start = StartDate.AddFrequency(Frequency, step);
            if (start > last)
                yield break;

            DateOnly nextStart = StartDate.AddFrequency(Frequency, step + 1);
            DateOnly slotEnd = nextStart.AddDays(-1);
            if (EndDate is { } scheduleEnd && slotEnd > scheduleEnd)
                slotEnd = scheduleEnd;

            yield return new ScheduleSlot(step, start, slotEnd);
        }
    }

    /// <summary>
    /// Looks ahead from the start date when the schedule hasn't begun yet, so a schedule
    /// starting far in the future still reports its first due date.
    /// </summary>
    public DateOnly? GetNextDueDate(DateOnly today)
    {
        DateOnly from = StartDate > today ? StartDate : today;
        return GetSlots(from.AddMonths(4))
            .Where(slot => slot.Start > today)
            .Select(slot => (DateOnly?)slot.Start)
            .FirstOrDefault();
    }

    /// <summary>
    /// Creates occurrences only for slots after <paramref name="lastCoveredDate"/>, so editing the
    /// start date never back-fills periods before what already exists.
    /// </summary>
    public List<TOccurrence> GenerateMissing(DateOnly? lastCoveredDate, DateOnly today, int primaryUserId) =>
        GetSlots(today)
            .Where(slot => lastCoveredDate is null || slot.Start > lastCoveredDate)
            .Select(slot => CreateOccurrence(slot, primaryUserId))
            .ToList();

    protected abstract TOccurrence CreateOccurrence(ScheduleSlot slot, int primaryUserId);
}
