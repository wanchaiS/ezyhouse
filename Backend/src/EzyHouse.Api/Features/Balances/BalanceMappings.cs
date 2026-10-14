using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.Balances;

public static class BalanceMappings
{
    public static SettlementDto ToDto(this Settlement settlement) => new(
        settlement.Id,
        CoreMappings.Ref(settlement.From, settlement.FromUserId),
        CoreMappings.Ref(settlement.To, settlement.ToUserId),
        settlement.Amount,
        settlement.Date,
        CoreMappings.Ref(settlement.CreatedBy, settlement.CreatedByUserId));

    public static BalancesDto ToBalancesDto(
        IReadOnlyDictionary<int, decimal> nets,
        IReadOnlyList<Debt> debts,
        IReadOnlyDictionary<int, User> users) => new(
        nets.Select(pair => new MemberBalanceDto(CoreMappings.Ref(UserOrNull(users, pair.Key), pair.Key), pair.Value)).ToList(),
        debts.Select(debt => new DebtDto(
            CoreMappings.Ref(UserOrNull(users, debt.FromUserId), debt.FromUserId),
            CoreMappings.Ref(UserOrNull(users, debt.ToUserId), debt.ToUserId),
            debt.Amount)).ToList());

    private static User? UserOrNull(IReadOnlyDictionary<int, User> users, int id) =>
        users.TryGetValue(id, out User? user) ? user : null;
}
