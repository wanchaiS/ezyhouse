using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Extensions;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Features.Bonds;

public sealed class BondRepository(EzyHouseDbContext db) : IBondRepository
{
    public Task<List<Bond>> ListAsync(int householdId) =>
        Bonds(householdId).OrderByDescending(bond => bond.PaidDate).ThenByDescending(bond => bond.Id).ToListAsync();

    public Task<Bond?> FindAsync(int householdId, int id) =>
        Bonds(householdId).FirstOrDefaultAsync(bond => bond.Id == id);

    public void Add(Bond bond) => db.Bonds.Add(bond);

    public void Remove(Bond bond) => db.Bonds.Remove(bond);

    public Task SaveChangesAsync() => db.SaveChangesAsync();

    private IQueryable<Bond> Bonds(int householdId) =>
        db.Bonds.ForHousehold(householdId)
            .Include(bond => bond.Contributions).ThenInclude(contribution => contribution.User);
}
