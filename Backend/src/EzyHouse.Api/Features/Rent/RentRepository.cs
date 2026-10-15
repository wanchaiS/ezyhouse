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

    // The rent it created stays: the database clears each expense's link to the schedule.
    public void RemoveSchedule(RentSchedule schedule) => db.RentSchedules.Remove(schedule);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
