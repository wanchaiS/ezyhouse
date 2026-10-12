using EzyHouse.Domain.Scheduling;

namespace EzyHouse.Api.Scheduling;

public static class OccurrenceGenerator
{
    public static List<TOccurrence> FindMissing<TOccurrence>(
        RecurringSchedule<TOccurrence> schedule,
        IReadOnlySet<DateOnly> existingStarts,
        DateOnly today,
        int primaryUserId) =>
        schedule.GetSlots(today)
            .Where(slot => !existingStarts.Contains(slot.Start))
            .Select(slot => schedule.CreateOccurrence(slot, primaryUserId))
            .ToList();
}
