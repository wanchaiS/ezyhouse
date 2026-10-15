using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.Rent;

public interface IRentRepository : IRepository
{
    Task<List<RentSchedule>> ListSchedulesAsync(int householdId);

    Task<RentSchedule?> FindScheduleAsync(int householdId, int id);

    void AddSchedule(RentSchedule schedule);

    void RemoveSchedule(RentSchedule schedule);
}
