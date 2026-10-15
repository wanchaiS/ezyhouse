using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.Bills;

public interface IBillRepository : IRepository
{
    Task<List<BillType>> ListTypesAsync(int householdId);

    Task<BillType?> FindTypeAsync(int householdId, int id);

    void AddType(BillType billType);

    void RemoveType(BillType billType);
}
