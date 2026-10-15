using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.Bills;

public sealed class BillService(
    IBillRepository bills,
    ICurrentUserContext currentUser,
    IScheduleRunner runner,
    IClock clock) : IBillService
{
    public async Task<IReadOnlyList<BillTypeDto>> ListTypesAsync()
    {
        Household household = await currentUser.RequireHouseholdAsync();
        List<BillType> types = await bills.ListTypesAsync(household.Id);
        return types.Select(type => type.ToDto()).ToList();
    }

    public async Task<BillTypeDto> CreateTypeAsync(BillTypeInput request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        var billType = new BillType { HouseholdId = household.Id };
        Apply(billType, request, household);

        // Starts at the first date on or after today; earlier dates are never created.
        billType.Begin(clock.Today);
        bills.AddType(billType);
        await bills.SaveChangesAsync();

        // If that first date is today, its $0 bill is created straight away rather than on the next hourly run.
        await runner.RunDueAsync(billType, household.PrimaryUserId);
        return billType.ToDto();
    }

    public async Task<BillTypeDto> UpdateTypeAsync(int id, BillTypeInput request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        BillType billType = await bills.FindTypeAsync(household.Id, id)
            ?? throw new NotFoundException("Bill type not found.");

        // Anything already due is created under the old settings first, so the edit only shapes what comes after.
        await runner.RunDueAsync(billType, household.PrimaryUserId);

        Apply(billType, request, household);
        billType.Reschedule(clock.Today);
        await bills.SaveChangesAsync();

        await runner.RunDueAsync(billType, household.PrimaryUserId);
        return billType.ToDto();
    }

    public async Task DeleteTypeAsync(int id)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        BillType billType = await bills.FindTypeAsync(household.Id, id)
            ?? throw new NotFoundException("Bill type not found.");
        bills.RemoveType(billType);
        await bills.SaveChangesAsync();
    }

    private static void Apply(BillType billType, BillTypeInput request, Household household)
    {
        if (request.EndDate is { } end && end < request.StartDate!.Value)
            throw new ValidationFailedException("The end date can't be before the start date.");

        // A bill split among nobody couldn't be created, so the bill type would never produce anything.
        List<int> requested = request.MemberIds!.Distinct().ToList();
        if (requested.Count == 0)
            throw new ValidationFailedException("Pick at least one person to share this bill.");

        List<User> members = household.Members.Where(member => requested.Contains(member.Id)).ToList();
        if (members.Count != requested.Count)
            throw new ValidationFailedException("Every bill type member must be a current household member.");

        billType.Name = request.Name!.Trim();
        billType.Frequency = request.Frequency!.Value;
        billType.StartDate = request.StartDate!.Value;
        billType.EndDate = request.EndDate;
        billType.Members.Clear();
        billType.Members.AddRange(members);
    }
}
