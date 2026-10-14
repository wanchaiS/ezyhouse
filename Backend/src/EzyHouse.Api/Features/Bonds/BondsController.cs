using Microsoft.AspNetCore.Mvc;

namespace EzyHouse.Api.Features.Bonds;

[ApiController]
[Route("api/bonds")]
public sealed class BondsController(IBondService bonds) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<BondDto>> List() => bonds.ListAsync();

    [HttpGet("{id:int}")]
    public Task<BondDto> Get(int id) => bonds.GetAsync(id);

    [HttpPost]
    public Task<BondDto> Create(BondInput request) => bonds.CreateAsync(request);

    [HttpPut("{id:int}")]
    public Task<BondDto> Update(int id, BondInput request) => bonds.UpdateAsync(id, request);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await bonds.DeleteAsync(id);
        return NoContent();
    }
}
