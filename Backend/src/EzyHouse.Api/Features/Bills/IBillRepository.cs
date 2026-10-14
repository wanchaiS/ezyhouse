using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.Bills;

public interface IBillRepository : IRepository
{
    Task<List<BillType>> ListTypesAsync(int householdId);

    Task<BillType?> FindTypeAsync(int householdId, int id);

    Task<List<Bill>> ListPendingBillsOfTypeAsync(int householdId, int billTypeId);

    void AddType(BillType billType);

    void RemoveType(BillType billType);

    void RemoveBills(IEnumerable<Bill> bills);

    Task<List<Bill>> ListBillsAsync(int householdId, BillStatus? status);

    Task<Bill?> FindBillAsync(int householdId, int id);
}
