namespace EzyHouse.Api.Features.Households;

public interface IHouseholdService
{
    Task<HouseholdDto> CreateAsync(CreateHouseholdRequest request);

    Task<HouseholdDto> GetAsync();

    Task<HouseholdDto> JoinAsync(JoinHouseholdRequest request);

    Task<HouseholdDto> TransferPrimaryAsync(TransferPrimaryRequest request);

    Task LeaveAsync(LeaveInput request);
}
