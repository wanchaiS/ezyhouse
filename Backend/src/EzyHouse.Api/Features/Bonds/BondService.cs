using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Exceptions;

namespace EzyHouse.Api.Features.Bonds;

public sealed class BondService(IBondRepository bonds, ICurrentUserContext currentUser) : IBondService
{
    public async Task<IReadOnlyList<BondDto>> ListAsync()
    {
        Household household = await currentUser.RequireHouseholdAsync();
        return (await bonds.ListAsync(household.Id)).Select(bond => bond.ToDto()).ToList();
    }

    public async Task<BondDto> GetAsync(int id)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        return (await LoadAsync(household.Id, id)).ToDto();
    }

    public async Task<BondDto> CreateAsync(BondInput request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        var bond = new Bond { HouseholdId = household.Id };
        Apply(bond, request, household);
        bonds.Add(bond);
        await bonds.SaveChangesAsync();
        return (await LoadAsync(household.Id, bond.Id)).ToDto();
    }

    public async Task<BondDto> UpdateAsync(int id, BondInput request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        Bond bond = await LoadAsync(household.Id, id);
        Apply(bond, request, household);
        await bonds.SaveChangesAsync();
        return (await LoadAsync(household.Id, id)).ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        bonds.Remove(await LoadAsync(household.Id, id));
        await bonds.SaveChangesAsync();
    }

    private async Task<Bond> LoadAsync(int householdId, int id) =>
        await bonds.FindAsync(householdId, id) ?? throw new NotFoundException("Bond not found.");

    // The new list replaces the old one entirely, so removed contributions are deleted.
    private static void Apply(Bond bond, BondInput request, Household household)
    {
        var memberIds = household.Members.Select(member => member.Id).ToHashSet();
        if (request.Contributions!.Any(contribution => !memberIds.Contains(contribution.UserId!.Value)))
            throw new ValidationFailedException("Every contribution must be from a current household member.");

        var contributions = request.Contributions!
            .Select(contribution => new BondContribution
            {
                UserId = contribution.UserId!.Value,
                User = household.Members.First(member => member.Id == contribution.UserId),
                Amount = contribution.Amount!.Value,
                Date = contribution.Date!.Value,
            })
            .ToList();
        bond.Update(request.Total!.Value, request.PaidDate!.Value, contributions);
    }
}
