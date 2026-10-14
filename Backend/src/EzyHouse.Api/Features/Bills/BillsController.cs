using EzyHouse.Api.Core;
using EzyHouse.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EzyHouse.Api.Features.Bills;

[ApiController]
[Route("api")]
public sealed class BillsController(IBillService bills) : ControllerBase
{
    [HttpGet("bill-types")]
    public Task<IReadOnlyList<BillTypeDto>> ListTypes() => bills.ListTypesAsync();

    [HttpPost("bill-types")]
    public Task<BillTypeDto> CreateType(BillTypeInput request) => bills.CreateTypeAsync(request);

    [HttpPut("bill-types/{id:int}")]
    public Task<BillTypeDto> UpdateType(int id, BillTypeInput request) => bills.UpdateTypeAsync(id, request);

    [HttpDelete("bill-types/{id:int}")]
    public async Task<IActionResult> DeleteType(int id)
    {
        await bills.DeleteTypeAsync(id);
        return NoContent();
    }

    [HttpGet("bills")]
    public Task<IReadOnlyList<BillDto>> List([FromQuery] BillStatus? status) => bills.ListAsync(status);

    [HttpPost("bills/{id:int}/submit")]
    public Task<BillDto> Submit(int id, SubmitBillInput request) => bills.SubmitAsync(id, request);

    [HttpPut("bills/{id:int}/shares/{userId:int}")]
    public Task<BillDto> SetSharePaid(int id, int userId, PaidRequest request) =>
        bills.SetSharePaidAsync(id, userId, request.Paid!.Value);
}
