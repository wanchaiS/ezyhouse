using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
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

    public Task<List<Bill>> ListPendingBillsOfTypeAsync(int householdId, int billTypeId) =>
        db.Bills.ForHousehold(householdId)
            .Where(bill => bill.BillTypeId == billTypeId && bill.Status == BillStatus.Pending)
            .ToListAsync();

    public void AddType(BillType billType) => db.BillTypes.Add(billType);

    public void RemoveType(BillType billType) => db.BillTypes.Remove(billType);

    public void RemoveBills(IEnumerable<Bill> bills) => db.Bills.RemoveRange(bills);

    public Task<List<Bill>> ListBillsAsync(int householdId, BillStatus? status)
    {
        IQueryable<Bill> query = WithDetails(db.Bills.ForHousehold(householdId));
        if (status is BillStatus wanted)
            query = query.Where(bill => bill.Status == wanted);

        return query.OrderByDescending(bill => bill.DueDate).ThenByDescending(bill => bill.Id).ToListAsync();
    }

    public Task<Bill?> FindBillAsync(int householdId, int id) =>
        WithDetails(db.Bills.ForHousehold(householdId))
            .Include(bill => bill.BillType!)
            .ThenInclude(billType => billType.Members)
            .FirstOrDefaultAsync(bill => bill.Id == id);

    public Task SaveChangesAsync() => db.SaveChangesAsync();

    private static IQueryable<Bill> WithDetails(IQueryable<Bill> query) =>
        query.Include(bill => bill.Payee)
            .Include(bill => bill.Shares)
            .ThenInclude(share => share.User);
}
