using EzyHouse.Api.Core;
using Microsoft.AspNetCore.Mvc;

namespace EzyHouse.Api.Features.Rent;

[ApiController]
[Route("api")]
public sealed class RentController(IRentService rent) : ControllerBase
{
    [HttpGet("rent-schedules")]
    public Task<IReadOnlyList<RentScheduleDto>> ListSchedules() => rent.ListSchedulesAsync();

    [HttpPost("rent-schedules")]
    public Task<RentScheduleDto> CreateSchedule(RentScheduleInput request) => rent.CreateScheduleAsync(request);

    [HttpPut("rent-schedules/{id:int}")]
    public Task<RentScheduleDto> UpdateSchedule(int id, RentScheduleInput request) => rent.UpdateScheduleAsync(id, request);

    [HttpDelete("rent-schedules/{id:int}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        await rent.DeleteScheduleAsync(id);
        return NoContent();
    }

    [HttpGet("rent-periods")]
    public Task<IReadOnlyList<RentPeriodDto>> ListPeriods() => rent.ListPeriodsAsync();

    [HttpPut("rent-periods/{id:int}/shares/{userId:int}")]
    public Task<RentPeriodDto> SetSharePaid(int id, int userId, PaidRequest request) =>
        rent.SetSharePaidAsync(id, userId, request.Paid!.Value);
}
