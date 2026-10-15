using Microsoft.AspNetCore.Mvc;

namespace EzyHouse.Api.Features.Bills;

[ApiController]
[Route("api/bill-types")]
public sealed class BillsController(IBillService bills) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<BillTypeDto>> ListTypes() => bills.ListTypesAsync();

    [HttpPost]
    public Task<BillTypeDto> CreateType(BillTypeInput request) => bills.CreateTypeAsync(request);

    [HttpPut("{id:int}")]
    public Task<BillTypeDto> UpdateType(int id, BillTypeInput request) => bills.UpdateTypeAsync(id, request);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteType(int id)
    {
        await bills.DeleteTypeAsync(id);
        return NoContent();
    }
}
