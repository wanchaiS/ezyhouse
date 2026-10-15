using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

    // The body may be left out: only a primary leaving with others still there has to send one.
    [HttpPost("leave")]
    public async Task<IActionResult> Leave([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] LeaveInput? request)
    {
        await households.LeaveAsync(request ?? new LeaveInput(null));
        return NoContent();
    }

    [HttpPut("primary")]
    public Task<HouseholdDto> TransferPrimary(TransferPrimaryRequest request) =>
        households.TransferPrimaryAsync(request);
}
