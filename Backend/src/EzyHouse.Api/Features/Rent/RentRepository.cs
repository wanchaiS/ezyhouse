using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Extensions;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Features.Rent;

public sealed class RentRepository(EzyHouseDbContext db) : IRentRepository
{
    public Task<List<RentSchedule>> ListSchedulesAsync(int householdId) =>
        db.RentSchedules.ForHousehold(householdId)
            .Include(schedule => schedule.Shares).ThenInclude(share => share.User)
            .OrderBy(schedule => schedule.StartDate)
            .ThenBy(schedule => schedule.Id)
            .ToListAsync();

    public Task<RentSchedule?> FindScheduleAsync(int householdId, int id) =>
        db.RentSchedules.ForHousehold(householdId)
            .Include(schedule => schedule.Shares).ThenInclude(share => share.User)
            .FirstOrDefaultAsync(schedule => schedule.Id == id);

    public void AddSchedule(RentSchedule schedule) => db.RentSchedules.Add(schedule);

    public void RemoveSchedule(RentSchedule schedule) => db.RentSchedules.Remove(schedule);

    public Task<List<RentPeriod>> ListPeriodsAsync(int householdId) =>
        PeriodsOf(householdId)
            .OrderByDescending(period => period.StartDate)
            .ThenByDescending(period => period.Id)
            .ToListAsync();

    public Task<RentPeriod?> FindPeriodAsync(int householdId, int id) =>
        PeriodsOf(householdId).FirstOrDefaultAsync(period => period.Id == id);

    // A period belongs to the household that owns its schedule.
    private IQueryable<RentPeriod> PeriodsOf(int householdId) =>
        db.RentPeriods
            .Where(period => db.RentSchedules.ForHousehold(householdId).Any(schedule => schedule.Id == period.ScheduleId))
            .Include(period => period.Payee)
            .Include(period => period.Shares).ThenInclude(share => share.User);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
