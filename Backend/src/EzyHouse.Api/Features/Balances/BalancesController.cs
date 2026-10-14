using Microsoft.AspNetCore.Mvc;

namespace EzyHouse.Api.Features.Balances;

[ApiController]
[Route("api")]
public sealed class BalancesController(IBalanceService balances) : ControllerBase
{
    [HttpGet("balances")]
    public Task<BalancesDto> GetBalances() => balances.GetBalancesAsync();

    [HttpGet("settlements")]
    public Task<IReadOnlyList<SettlementDto>> ListSettlements() => balances.ListSettlementsAsync();

    [HttpPost("settlements")]
    public Task<SettlementDto> CreateSettlement(SettlementInput request) => balances.CreateSettlementAsync(request);
}
