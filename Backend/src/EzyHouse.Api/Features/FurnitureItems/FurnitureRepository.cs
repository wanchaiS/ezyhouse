using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Extensions;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Features.FurnitureItems;

public sealed class FurnitureRepository(EzyHouseDbContext db) : IFurnitureRepository
{
    public async Task<IReadOnlyList<Furniture>> ListAsync(int householdId, FurnitureStatus? status)
    {
        IQueryable<Furniture> query = WithDetails(householdId);
        if (status is FurnitureStatus wanted)
            query = query.Where(item => item.Status == wanted);

        return await query.OrderBy(item => item.Id).ToListAsync();
    }

    public Task<Furniture?> FindAsync(int householdId, int id) =>
        WithDetails(householdId).FirstOrDefaultAsync(item => item.Id == id);

    public void Add(Furniture furniture) => db.Furniture.Add(furniture);

    public void Remove(Furniture furniture) => db.Furniture.Remove(furniture);

    public Task SaveChangesAsync() => db.SaveChangesAsync();

    private IQueryable<Furniture> WithDetails(int householdId) =>
        db.Furniture
            .ForHousehold(householdId)
            .Include(item => item.CreatedBy)
            .Include(item => item.Transactions).ThenInclude(transaction => transaction.Counterparty)
            .Include(item => item.Transactions).ThenInclude(transaction => transaction.Shares).ThenInclude(share => share.User)
            .AsSplitQuery();
}
