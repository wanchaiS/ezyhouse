using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Extensions;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Features.Bills;

public sealed class BillRepository(EzyHouseDbContext db) : IBillRepository
{
    public Task<List<BillType>> ListTypesAsync(int householdId) =>
        db.BillTypes.ForHousehold(householdId)
            .Include(billType => billType.Members)
            .OrderBy(billType => billType.Name)
            .ToListAsync();

    public Task<BillType?> FindTypeAsync(int householdId, int id) =>
        db.BillTypes.ForHousehold(householdId)
            .Include(billType => billType.Members)
            .FirstOrDefaultAsync(billType => billType.Id == id);

    public void AddType(BillType billType) => db.BillTypes.Add(billType);

    // The bills it created stay: the database clears each expense's link to the bill type.
    public void RemoveType(BillType billType) => db.BillTypes.Remove(billType);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
