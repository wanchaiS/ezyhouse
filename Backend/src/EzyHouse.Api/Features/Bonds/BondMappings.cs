using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Features.Bonds;

public static class BondMappings
{
    public static ContributionDto ToDto(this BondContribution contribution) =>
        new(CoreMappings.Ref(contribution.User, contribution.UserId), contribution.Amount, contribution.Date);

    public static BondDto ToDto(this Bond bond) => new(
        bond.Id,
        bond.Total,
        bond.PaidDate,
        bond.Contributions.Select(contribution => contribution.ToDto()).ToList(),
        bond.Outstanding);
}
