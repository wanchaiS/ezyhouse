namespace EzyHouse.Domain.Interfaces;

/// <summary>Anything stored per household, so queries can be scoped to the caller's household.</summary>
public interface IHouseholdOwned
{
    int HouseholdId { get; }
}
