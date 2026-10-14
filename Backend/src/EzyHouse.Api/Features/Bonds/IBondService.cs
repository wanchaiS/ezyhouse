using EzyHouse.Api.Dtos;

namespace EzyHouse.Api.Services;

public interface IBondService
{
    Task<IReadOnlyList<BondDto>> ListAsync();

    Task<BondDto> GetAsync(int id);

    Task<BondDto> CreateAsync(BondInput request);

    Task<BondDto> UpdateAsync(int id, BondInput request);

    Task DeleteAsync(int id);
}
