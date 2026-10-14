using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Core;

/// <summary>
/// Creates the rent periods and bill placeholders that are due up to today. There is no background
/// timer (API.md): entries are created when a list is read, and before the leave check.
/// Call it before making any other changes in the request — it saves what it adds.
/// </summary>
public interface IRecurringChargeGenerator
{
    Task GenerateRentPeriodsAsync(Household household, DateOnly today);

    Task GenerateBillPlaceholdersAsync(Household household, DateOnly today);
}
