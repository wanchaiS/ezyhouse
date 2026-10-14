namespace EzyHouse.Api.Features.Balances;

public interface IBalanceService
{
    Task<BalancesDto> GetBalancesAsync();

    Task<IReadOnlyList<SettlementDto>> ListSettlementsAsync();

    Task<SettlementDto> CreateSettlementAsync(SettlementInput request);
}
