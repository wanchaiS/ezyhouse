using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Extensions;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Features.Households;

public sealed class HouseholdRepository(EzyHouseDbContext db, IHouseholdBalances balances) : IHouseholdRepository
{
    public Task<bool> InviteCodeExistsAsync(string inviteCode) =>
        db.Households.AnyAsync(household => household.InviteCode == inviteCode);

    public Task<Household?> FindByInviteCodeAsync(string inviteCode) =>
        WithMembers().FirstOrDefaultAsync(household => household.InviteCode == inviteCode);

    public Task<Household?> FindAsync(int householdId) =>
        WithMembers().FirstOrDefaultAsync(household => household.Id == householdId);

    public void Add(Household household) => db.Households.Add(household);

    public async Task<LeaveSituation> GetLeaveSituationAsync(Household household, int userId, DateOnly today)
    {
        var (_, debts) = await balances.CalculateAsync(household.Id, household.Members.Select(member => member.Id));

        List<RentPeriod> unpaidRentPeriods = await db.RentPeriods.AsNoTracking()
            .Where(period => period.Schedule!.HouseholdId == household.Id)
            .Where(period => period.Shares.Any(share => !share.Paid))
            .ToListAsync();

        List<RentSchedule> rentSchedules = await db.RentSchedules.AsNoTracking()
            .ForHousehold(household.Id)
            .ToListAsync();

        List<Bill> unpaidBills = await db.Bills.AsNoTracking()
            .ForHousehold(household.Id)
            .Where(bill => bill.Shares.Any(share => !share.Paid))
            .ToListAsync();

        return new LeaveSituation(
            userId,
            household.PrimaryUserId == userId,
            household.Members.Count,
            today,
            debts,
            unpaidRentPeriods,
            rentSchedules,
            unpaidBills);
    }

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
