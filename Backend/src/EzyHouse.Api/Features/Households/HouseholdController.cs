using Microsoft.AspNetCore.Mvc;

namespace EzyHouse.Api.Features.Households;

[ApiController]
[Route("api/household")]
public sealed class HouseholdController(IHouseholdService households) : ControllerBase
{
    [HttpPost]
    public Task<HouseholdDto> Create(CreateHouseholdRequest request) => households.CreateAsync(request);

    [HttpGet]
    public Task<HouseholdDto> Get() => households.GetAsync();

    [HttpPost("join")]
    public Task<HouseholdDto> Join(JoinHouseholdRequest request) => households.JoinAsync(request);

    [HttpPost("leave")]
    public async Task<IActionResult> Leave()
    {
        await households.LeaveAsync();
        return NoContent();
    }

    [HttpPut("primary")]
    public Task<HouseholdDto> TransferPrimary(TransferPrimaryRequest request) =>
        households.TransferPrimaryAsync(request);
}
