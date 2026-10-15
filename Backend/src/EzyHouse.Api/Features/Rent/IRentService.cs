namespace EzyHouse.Api.Features.Rent;

public interface IRentService
{
    Task<IReadOnlyList<RentScheduleDto>> ListSchedulesAsync();

    Task<RentScheduleDto> CreateScheduleAsync(RentScheduleInput request);

    Task<RentScheduleDto> UpdateScheduleAsync(int id, RentScheduleInput request);

    Task DeleteScheduleAsync(int id);
}
