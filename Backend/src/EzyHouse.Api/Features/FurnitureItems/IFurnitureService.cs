using EzyHouse.Domain.Enums;

namespace EzyHouse.Api.Features.FurnitureItems;

public interface IFurnitureService
{
    Task<IReadOnlyList<FurnitureDto>> ListAsync(FurnitureStatus? status);

    Task<FurnitureDto> GetAsync(int id);

    Task<FurnitureDto> CreateAsync(FurnitureInput request);

    Task<FurnitureDto> UpdateAsync(int id, FurnitureInput request);

    Task DeleteAsync(int id);

    Task<FurnitureDto> SellAsync(int id, SaleInput request);

    Task<FurnitureDto> DisposeAsync(int id, DisposalInput request);
}
