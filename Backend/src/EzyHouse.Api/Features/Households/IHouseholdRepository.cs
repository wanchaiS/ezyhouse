using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.Households;

public interface IHouseholdRepository : IRepository
{
    Task<bool> InviteCodeExistsAsync(string inviteCode);

    /// <summary>The household with its members and primary loaded.</summary>
    Task<Household?> FindByInviteCodeAsync(string inviteCode);

    /// <summary>The household with its members and primary loaded.</summary>
    Task<Household?> FindAsync(int householdId);

    void Add(Household household);

    /// <summary>The household's rent schedules that include the user in their split, with their shares loaded.</summary>
    Task<List<RentSchedule>> GetRentSchedulesWithShareAsync(int householdId, int userId);

    /// <summary>The household's bill types that list the user as a member, with their members loaded.</summary>
    Task<List<BillType>> GetBillTypesWithMemberAsync(int householdId, int userId);
}
