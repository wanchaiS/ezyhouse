using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Features.Households;

public static class HouseholdMappings
{
    public static HouseholdDto ToDto(this Household household) => new(
        household.Id,
        household.Name,
        household.InviteCode,
        CoreMappings.Ref(household.PrimaryUser, household.PrimaryUserId),
        household.Members.Select(member => member.ToRef()).ToList());
}
