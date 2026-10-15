using Microsoft.AspNetCore.Mvc;

namespace EzyHouse.Api.Features.Rent;

[ApiController]
[Route("api/rent-schedules")]
public sealed class RentController(IRentService rent) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<RentScheduleDto>> ListSchedules() => rent.ListSchedulesAsync();

    [HttpPost]
    public Task<RentScheduleDto> CreateSchedule(RentScheduleInput request) => rent.CreateScheduleAsync(request);

    [HttpPut("{id:int}")]
    public Task<RentScheduleDto> UpdateSchedule(int id, RentScheduleInput request) => rent.UpdateScheduleAsync(id, request);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        await rent.DeleteScheduleAsync(id);
        return NoContent();
    }
}
