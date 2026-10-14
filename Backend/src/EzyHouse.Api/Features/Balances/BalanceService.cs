using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.Balances;

public sealed class BalanceService(
    ICurrentUserContext currentUser,
    IHouseholdBalances balances,
    ISettlementRepository settlements,
    IClock clock) : IBalanceService
{
    public async Task<BalancesDto> GetBalancesAsync()
    {
        Household household = await currentUser.RequireHouseholdAsync();
        var (nets, debts) = await balances.CalculateAsync(household.Id, household.Members.Select(member => member.Id));

        // Debts can still name someone who has left the household, so look up everyone mentioned.
        IEnumerable<int> mentioned = nets.Keys
            .Concat(debts.Select(debt => debt.FromUserId))
            .Concat(debts.Select(debt => debt.ToUserId));
        IReadOnlyDictionary<int, User> users = await settlements.FindUsersAsync(mentioned);

        return BalanceMappings.ToBalancesDto(nets, debts, users);
    }

    public async Task<IReadOnlyList<SettlementDto>> ListSettlementsAsync()
    {
        Household household = await currentUser.RequireHouseholdAsync();
        IReadOnlyList<Settlement> list = await settlements.ListAsync(household.Id);
        return list.Select(settlement => settlement.ToDto()).ToList();
    }

    public async Task<SettlementDto> CreateSettlementAsync(SettlementInput request)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        int fromId = request.FromUserId!.Value, toId = request.ToUserId!.Value;

        if (fromId == toId)
            throw new ValidationFailedException("A settlement must be between two different people.");

        User? from = household.Members.FirstOrDefault(member => member.Id == fromId);
        User? to = household.Members.FirstOrDefault(member => member.Id == toId);
        if (from is null || to is null)
            throw new ValidationFailedException("Both people in a settlement must be current members of the household.");

        if (currentUser.UserId != fromId && currentUser.UserId != toId)
            throw new ForbiddenException("Only the person paying or the person being paid can record a settlement.");

        var settlement = new Settlement
        {
            HouseholdId = household.Id,
            FromUserId = fromId,
            From = from,
            ToUserId = toId,
            To = to,
            Amount = request.Amount!.Value,
            Date = clock.Today,
            CreatedByUserId = currentUser.UserId,
            CreatedBy = await currentUser.GetUserAsync(),
        };
        settlements.Add(settlement);
        await settlements.SaveChangesAsync();

        return settlement.ToDto();
    }
}
