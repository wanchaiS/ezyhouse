using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Domain.Scheduling;

/// <summary>
/// A schedule that creates one expense per date. The steps for finding due dates are fixed here;
/// each subclass only decides what the expense for one date looks like (<see cref="CreateOccurrence"/>).
/// </summary>
public abstract class RecurringSchedule<TOccurrence> : IHouseholdOwned
    where TOccurrence : Expense
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }

    public Household? Household { get; set; }

    public Frequency Frequency { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// The next date to create an expense for. It only ever moves forward while the settings stay the
    /// same. Null once the schedule has ended.
    /// </summary>
    public DateOnly? NextDueDate { get; set; }

    /// <summary>
    /// The latest date this schedule has created an expense for; null until the first one. It is kept
    /// even when that expense is deleted, so no edit can bring the date, or any earlier one, back.
    /// </summary>
    public DateOnly? LastCreatedDate { get; set; }

    /// <summary>
    /// Yields each slot from <see cref="StartDate"/> until <see cref="EndDate"/>, or forever when there is
    /// no end date. Dates are always calculated from the start plus a step count, so a monthly schedule
    /// starting on the 31st comes back to the 31st after a short month.
    /// </summary>
    public IEnumerable<ScheduleSlot> GetSlots()
    {
        for (int step = 0; ; step++)
        {
            DateOnly start = StartDate.AddFrequency(Frequency, step);
            if (EndDate is { } end && start > end)
                yield break;

            DateOnly slotEnd = StartDate.AddFrequency(Frequency, step + 1).AddDays(-1);
            if (EndDate is { } scheduleEnd && slotEnd > scheduleEnd)
                slotEnd = scheduleEnd;

            yield return new ScheduleSlot(step, start, slotEnd);
        }
    }

    /// <summary>The first schedule date on or after <paramref name="date"/>, or null when the schedule ends before it.</summary>
    public DateOnly? FirstDateOnOrAfter(DateOnly date) =>
        GetSlots()
            .Where(slot => slot.Start >= date)
            .Select(slot => (DateOnly?)slot.Start)
            .FirstOrDefault();

    /// <summary>A new schedule starts at its first date on or after today; earlier dates are never created.</summary>
    public void Begin(DateOnly today) => NextDueDate = FirstDateOnOrAfter(today);

    /// <summary>
    /// After an edit, the next date comes from the new settings: the first on or after today, and never
    /// on or before a date the schedule has already created. A schedule that hasn't created anything yet
    /// can therefore be moved to start earlier, as long as it isn't before today.
    /// </summary>
    public void Reschedule(DateOnly today)
    {
        DateOnly from = LastCreatedDate is { } last && last >= today ? last.AddDays(1) : today;
        NextDueDate = FirstDateOnOrAfter(from);
    }

    /// <summary>
    /// Takes every date from <see cref="NextDueDate"/> up to and including today, oldest first, and moves
    /// <see cref="NextDueDate"/> past today. Dates missed while nothing ran are each returned with their own date.
    /// </summary>
    public IReadOnlyList<ScheduleSlot> TakeDueSlots(DateOnly today)
    {
        if (NextDueDate is not { } next || next > today)
            return [];

        List<ScheduleSlot> due = [];
        ScheduleSlot? following = null;
        foreach (ScheduleSlot slot in GetSlots().SkipWhile(slot => slot.Start < next))
        {
            if (slot.Start > today)
            {
                following = slot;
                break;
            }

            due.Add(slot);
        }

        NextDueDate = following?.Start;
        if (due.Count > 0)
            LastCreatedDate = due[^1].Start;
        return due;
    }

    /// <summary>
    /// The template: every due date becomes one expense, paid by <paramref name="primaryUserId"/>,
    /// the household's primary at the moment the expenses are created.
    /// </summary>
    public IReadOnlyList<TOccurrence> TakeDueOccurrences(DateOnly today, int primaryUserId) =>
        TakeDueSlots(today).Select(slot => CreateOccurrence(slot, primaryUserId)).ToList();

    /// <summary>Whether there is anyone to split a created expense between.</summary>
    public abstract bool HasAnyoneToShare { get; }

    /// <summary>The factory step each subclass fills in: the expense for one date.</summary>
    protected abstract TOccurrence CreateOccurrence(ScheduleSlot slot, int primaryUserId);
}
