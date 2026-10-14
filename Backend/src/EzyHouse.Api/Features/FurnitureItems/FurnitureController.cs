using EzyHouse.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EzyHouse.Api.Features.FurnitureItems;

[ApiController]
[Route("api/furniture")]
public sealed class FurnitureController(IFurnitureService furniture) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<FurnitureDto>> List([FromQuery] FurnitureStatus? status) => furniture.ListAsync(status);

    [HttpGet("{id:int}")]
    public Task<FurnitureDto> Get(int id) => furniture.GetAsync(id);

    [HttpPost]
    public Task<FurnitureDto> Create(FurnitureInput request) => furniture.CreateAsync(request);

    [HttpPut("{id:int}")]
    public Task<FurnitureDto> Update(int id, FurnitureInput request) => furniture.UpdateAsync(id, request);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await furniture.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/sell")]
    public Task<FurnitureDto> Sell(int id, SaleInput request) => furniture.SellAsync(id, request);

    [HttpPost("{id:int}/dispose")]
    public Task<FurnitureDto> Dispose(int id, DisposalInput request) => furniture.DisposeAsync(id, request);
}
