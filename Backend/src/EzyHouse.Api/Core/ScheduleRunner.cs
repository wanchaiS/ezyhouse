using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Extensions;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.Scheduling;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Core;

public sealed class ScheduleRunner(EzyHouseDbContext db, IClock clock, ILogger<ScheduleRunner> logger) : IScheduleRunner
{
    public Task RunDueAsync(RentSchedule schedule, int primaryUserId) => CreateDueAsync(schedule, primaryUserId);

    public Task RunDueAsync(BillType billType, int primaryUserId) => CreateDueAsync(billType, primaryUserId);

    public Task RunDueForHouseholdAsync(int householdId) =>
        RunEveryDueAsync(db.RentSchedules.ForHousehold(householdId), db.BillTypes.ForHousehold(householdId));

    public Task RunDueForEveryHouseholdAsync() =>
        RunEveryDueAsync(db.RentSchedules, db.BillTypes);

    private async Task RunEveryDueAsync(IQueryable<RentSchedule> rentSchedules, IQueryable<BillType> billTypes)
    {
        DateOnly today = clock.Today;

        // The household is loaded for its current primary, who pays everything created now.
        List<RentSchedule> dueRent = await rentSchedules
            .Where(schedule => schedule.NextDueDate != null && schedule.NextDueDate <= today)
            .Include(schedule => schedule.Household)
            .ToListAsync();
        foreach (RentSchedule schedule in dueRent)
            await RunDueAsync(schedule, schedule.Household!.PrimaryUserId);

        List<BillType> dueBills = await billTypes
            .Where(billType => billType.NextDueDate != null && billType.NextDueDate <= today)
            .Include(billType => billType.Members)
            .Include(billType => billType.Household)
            .ToListAsync();
        foreach (BillType billType in dueBills)
            await RunDueAsync(billType, billType.Household!.PrimaryUserId);
    }

    // Each schedule is saved on its own, so a problem with one never holds back the others.
    private async Task CreateDueAsync<TOccurrence>(RecurringSchedule<TOccurrence> schedule, int primaryUserId)
        where TOccurrence : Expense
    {
        // Input validation stops this, but an expense split among nobody can't be saved, so a schedule
        // that somehow has nobody is left alone rather than failing every run.
        if (!schedule.HasAnyoneToShare)
        {
            logger.LogWarning("Skipped {Schedule} {Id}: nobody to share its expenses.", schedule.GetType().Name, schedule.Id);
            return;
        }

        IReadOnlyList<TOccurrence> due = schedule.TakeDueOccurrences(clock.Today, primaryUserId);
        if (due.Count == 0)
            return;

        db.Expenses.AddRange(due);
        await SaveIgnoringDuplicatesAsync();
    }

    // Two runs at the same moment (the hourly job and a request) can both create the same date. The
    // unique indexes (RentScheduleId, Date) and (BillTypeId, Date) reject the second insert, which is
    // fine: the other run has already saved those expenses and moved the next due date on, so this
    // run's inserts are dropped and the schedule (its next due and last created dates) is read back as
    // the other run left it.
    private async Task SaveIgnoringDuplicatesAsync()
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            foreach (var entry in db.ChangeTracker.Entries().ToList())
            {
                if (entry.State == EntityState.Added)
                    entry.State = EntityState.Detached;
                else if (entry.State == EntityState.Modified)
                    await entry.ReloadAsync();
            }
        }
    }
}
