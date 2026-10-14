using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.FurnitureItems;

public interface IFurnitureRepository : IRepository
{
    Task<IReadOnlyList<Furniture>> ListAsync(int householdId, FurnitureStatus? status);

    Task<Furniture?> FindAsync(int householdId, int id);

    void Add(Furniture furniture);

    void Remove(Furniture furniture);
}
