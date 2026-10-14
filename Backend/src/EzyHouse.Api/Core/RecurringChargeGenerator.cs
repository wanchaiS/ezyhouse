using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Extensions;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Core;

public sealed class RecurringChargeGenerator(EzyHouseDbContext db) : IRecurringChargeGenerator
{
    public async Task GenerateRentPeriodsAsync(Household household, DateOnly today)
    {
        List<RentSchedule> schedules = await db.RentSchedules.ForHousehold(household.Id).ToListAsync();
        foreach (RentSchedule schedule in schedules)
        {
            // Forward-only: nothing is created before the end of the last existing period.
            DateOnly? lastCovered = await db.RentPeriods
                .Where(period => period.ScheduleId == schedule.Id)
                .MaxAsync(period => (DateOnly?)period.EndDate);
            db.RentPeriods.AddRange(schedule.GenerateMissing(lastCovered, today, household.PrimaryUserId));
        }

        await SaveIgnoringDuplicatesAsync();
    }

    public async Task GenerateBillPlaceholdersAsync(Household household, DateOnly today)
    {
        List<BillType> billTypes = await db.BillTypes.ForHousehold(household.Id).ToListAsync();
        foreach (BillType billType in billTypes)
        {
            DateOnly? lastCovered = await db.Bills
                .Where(bill => bill.BillTypeId == billType.Id)
                .MaxAsync(bill => (DateOnly?)bill.DueDate);
            db.Bills.AddRange(billType.GenerateMissing(lastCovered, today, household.PrimaryUserId));
        }

        await SaveIgnoringDuplicatesAsync();
    }

    // Two requests reading at the same moment can both try to create the same entry. The unique
    // indexes (ScheduleId, StartDate) and (BillTypeId, DueDate) reject the second insert, which is
    // fine: the other request has already saved it, so the pending inserts are simply dropped.
    private async Task SaveIgnoringDuplicatesAsync()
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            foreach (var entry in db.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Added).ToList())
                entry.State = EntityState.Detached;
        }
    }
}
