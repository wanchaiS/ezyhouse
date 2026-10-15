using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Core;

/// <summary>
/// Creates the expenses that rent schedules and bill types have fallen due for, up to today, each paid
/// by whoever is primary when it runs. The background job calls it hourly for every household; the
/// Rent, Bills and Households slices call it so a change takes effect at once.
/// Call it before making any other changes in the request — it saves what it adds.
/// </summary>
public interface IScheduleRunner
{
    /// <summary>The schedule must be tracked, with its shares loaded.</summary>
    Task RunDueAsync(RentSchedule schedule, int primaryUserId);

    /// <summary>The bill type must be tracked, with its members loaded.</summary>
    Task RunDueAsync(BillType billType, int primaryUserId);

    Task RunDueForHouseholdAsync(int householdId);

    Task RunDueForEveryHouseholdAsync();
}
