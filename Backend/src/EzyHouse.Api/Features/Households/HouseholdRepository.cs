using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Extensions;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Features.Households;

public sealed class HouseholdRepository(EzyHouseDbContext db) : IHouseholdRepository
{
    public Task<bool> InviteCodeExistsAsync(string inviteCode) =>
        db.Households.AnyAsync(household => household.InviteCode == inviteCode);

    public Task<Household?> FindByInviteCodeAsync(string inviteCode) =>
        WithMembers().FirstOrDefaultAsync(household => household.InviteCode == inviteCode);

    public Task<Household?> FindAsync(int householdId) =>
        WithMembers().FirstOrDefaultAsync(household => household.Id == householdId);

    public void Add(Household household) => db.Households.Add(household);

    public Task<List<RentSchedule>> GetRentSchedulesWithShareAsync(int householdId, int userId) =>
        db.RentSchedules
            .ForHousehold(householdId)
            .Where(schedule => schedule.Shares.Any(share => share.UserId == userId))
            .ToListAsync();

    public Task<List<BillType>> GetBillTypesWithMemberAsync(int householdId, int userId) =>
        db.BillTypes
            .ForHousehold(householdId)
            .Where(billType => billType.Members.Any(member => member.Id == userId))
            .Include(billType => billType.Members)
            .ToListAsync();

    public Task SaveChangesAsync() => db.SaveChangesAsync();

    private IQueryable<Household> WithMembers() =>
        db.Households
            .Include(household => household.Members)
            .Include(household => household.PrimaryUser);
}
