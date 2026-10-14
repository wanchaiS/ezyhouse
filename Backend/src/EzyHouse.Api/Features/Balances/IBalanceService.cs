using EzyHouse.Api.Dtos;

namespace EzyHouse.Api.Services;

public interface IBalanceService
{
    Task<BalancesDto> GetBalancesAsync();

    Task<IReadOnlyList<SettlementDto>> ListSettlementsAsync();

    Task<SettlementDto> CreateSettlementAsync(SettlementInput request);
}
