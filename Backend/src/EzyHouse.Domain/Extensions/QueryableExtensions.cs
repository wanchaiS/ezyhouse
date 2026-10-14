using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Domain.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ForHousehold<T>(this IQueryable<T> query, int householdId)
        where T : IHouseholdOwned =>
        query.Where(item => item.HouseholdId == householdId);
}
